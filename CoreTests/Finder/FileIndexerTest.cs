// HwpFileDecoderTest.cs
// 이 파일은 CoreTests의 일부입니다.
// 
// © 2024 Potados <song@potados.com>
// 
// CoreTests은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using Core.Finder;
using CoreTests.Lib;
using Xunit.Abstractions;

namespace CoreTests.Finder;

public class FileIndexerTest
{
    public FileIndexerTest(ITestOutputHelper output)
    {
        Console.SetOut(new ConsoleWriter(output));
    }
    
    [Fact]
    public void Index_ShouldIndex()
    {
        using var indexer = new FileIndexer();
        
        indexer.Index(@"C:\Users\Administrator\Desktop\모든 국민은 신속한 재판을 받을 권리를.hwp");
        indexer.Index(@"C:\Users\Administrator\Desktop\국가는 주택개발정책등을 통하여.hwp");
    }

    [Fact]
    public void Search_ShouldFind()
    {
        using var indexer = new FileIndexer();
        
        var results = indexer.Search("는 6년으로");
        Console.WriteLine($"{results.ScoreDocs.Length} results found.");
        foreach (var result in results.ScoreDocs)
        {  
            var doc = indexer.GetDocument(result.Doc);
            Console.WriteLine($"Score: {result.Score}, Path: {doc.GetField("path").GetStringValue()}");
        }
    }
}