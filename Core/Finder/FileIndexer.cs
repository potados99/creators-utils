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
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace Core.Finder;

public class FileIndexer<T> where T : FileDecoder, new()
{
    private readonly FSDirectory _dir;
    private readonly IndexWriter _writer;
    private readonly IndexReader _reader;
    private readonly IndexSearcher _searcher;

    public FileIndexer()
    {
        const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var indexPath = Path.Combine(basePath, "index");

        var analyzer = new StandardAnalyzer(AppLuceneVersion);
        var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
        indexConfig.OpenMode = OpenMode.CREATE_OR_APPEND;
        
        _dir = FSDirectory.Open(indexPath);
        _writer = new IndexWriter(_dir, indexConfig);
        _reader = _writer.GetReader(applyAllDeletes: false);
        _searcher = new IndexSearcher(_reader);
    }

    ~FileIndexer()
    {
        Close();
    }

    public void Index(string path)
    {
        Index(new FileInfo(path));
    }
    
    public void Index(FileInfo original)
    {
        using var reader = _writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);
        var results = searcher.Search(new TermQuery(new Term("path", original.FullName)), 1);
        if (results.TotalHits > 0)
        {
            var found = searcher.Doc(results.ScoreDocs[0].Doc);
            var modified = found.GetField("modified").GetInt64ValueOrDefault();
            if (modified >= original.LastWriteTimeUtc.Ticks)
            {
                // 이미 해당 파일에 대해 최신 인덱스가 존재합니다.
                // 따라서 패스합니다.
                Console.Out.WriteLine($"Already indexed: {original.FullName}");
                return;
            }
        }
        else
        {
            Console.Out.WriteLine($"Nothing found.");
        }

        Console.Out.WriteLine($"Indexing: {original.FullName}");
        
        var decoded = new T().Decode(original.FullName, Path.Combine(original.DirectoryName ?? "", $".{original.Name}.txt"));

        var doc = new Document();

        using var fs = new FileStream(decoded.FullName, FileMode.Open, FileAccess.Read);

        doc.Add(new StringField("path", original.FullName, Field.Store.YES));
        doc.Add(new Int64Field("modified", original.LastWriteTimeUtc.Ticks, Field.Store.YES));
        doc.Add(new TextField("contents", new StreamReader(fs, Encoding.UTF8)));

        _writer.UpdateDocument(new Term("path", original.FullName), doc);

        _writer.Flush(triggerMerge: false, applyAllDeletes: false);
        
        decoded.Delete();
    }

    public void Close()
    {
        _writer.Dispose();
        
        _dir.Dispose();
        _reader.Dispose();
    }
}