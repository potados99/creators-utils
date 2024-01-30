// FileIndexer.cs
// 이 파일은 Core의 일부입니다.
// 
// © 2024 Potados <song@potados.com>
// 
// Core은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System.Text;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace Core.Finder;

public class FileIndexer : LuceneComponent
{
    public void Index(string path)
    {
        Index(new FileInfo(path));
    }

    public void Index(FileInfo file)
    {
        if (!IsIndexed(file))
        {
            PerformIndex(file);
        }
    }

    private bool IsIndexed(FileInfo original)
    {
        using var reader = Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);
        var results = searcher.Search(new TermQuery(new Term("path", original.FullName)), 1);
        if (results.TotalHits > 0)
        {
            var found = searcher.Doc(results.ScoreDocs[0].Doc);
            var modified = found.GetField("modified").GetInt64ValueOrDefault();
            if (modified >= original.LastWriteTimeUtc.Ticks)
            {
                return true;
            }
        }

        return false;
    }

    private void PerformIndex(FileInfo original)
    {
        var decoded = GetDecoder(original).Decode(original.FullName);

        using var fs = new FileStream(decoded.FullName, FileMode.Open, FileAccess.Read);
        var doc = new Document
        {
            new StringField("path", original.FullName, Field.Store.YES),
            new Int64Field("modified", original.LastWriteTimeUtc.Ticks, Field.Store.YES),
            new TextField("contents", new StreamReader(fs, Encoding.UTF8))
        };
        
        Writer.UpdateDocument(new Term("path", original.FullName), doc);
        Writer.Flush(triggerMerge: false, applyAllDeletes: false);

        decoded.Delete();
    }

    private FileDecoder GetDecoder(FileInfo file)
    {
        var extension = file.Extension.ToLower();

        return extension switch
        {
            ".hwp" => new HwpFileDecoder(),
            _ => throw new NotSupportedException($"Extension {extension} is not supported.")
        };
    }

    public TopDocs Search(string keyword, int maxResults = 10)
    {
        using var reader = Writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);
        var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);
        var queryParser = new QueryParser(LuceneVersion.LUCENE_48, "contents", analyzer);
        var query = queryParser.Parse(keyword);
        
        return searcher.Search(query, maxResults);;
    }

    public Document GetDocument(int docId)
    {
        return Writer.GetReader(applyAllDeletes: true).Document(docId);
    }
}