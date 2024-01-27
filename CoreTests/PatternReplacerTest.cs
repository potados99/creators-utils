// RegexGeneratorTest.cs
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

using Core.Renamer;
using CoreTests.Lib;
using Xunit.Abstractions;

namespace CoreTests;

public class PatternReplacerTest
{
    public PatternReplacerTest(ITestOutputHelper output)
    {
        Console.SetOut(new ConsoleWriter(output));
    }

    [Fact]
    public void Replace_ShouldUnderstandNamedGroups()
    {
        const string capture = "안녕_세상_{회차}.hwpx"; // 이 패턴을
        const string replacement = "히히_세로운_세상_{회차}.hwpx"; // 이 패턴으로 바꾸는 것입니다.
        
        var replacer = new PatternReplacer(capture, replacement);

        const string original = "안녕_세상_1.hwpx"; // 이 파일 이름을 넣으면
        const string expected = "히히_세로운_세상_1.hwpx"; // 이렇게 나와야 합니다.

        Assert.Equal(expected, replacer.Replace(original));
    }
}