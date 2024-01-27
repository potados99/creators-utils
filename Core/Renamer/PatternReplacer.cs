// FileRenamer.cs
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

using System.Text.RegularExpressions;

namespace Core.Renamer;

public class PatternReplacer
{
    public PatternReplacer(string capture, string replacement)
    {
        _capture = capture;
        _replacement = replacement;
    }

    private readonly string _capture; // "안녕_세상_{회차}.hwpx"
    private readonly string _replacement; // "히히_세로운_세상_{회차}.hwpx"

    public string Replace(string oldName)
    {
        var capture = Regex.Replace(_capture, "{(.+)}", "(?<$1>.+)"); // 안녕_세상_(?<회차>.+).hwpx
        var replacement = Regex.Replace(_replacement, "{(.+)}", "${${1}}"); // 히히_세로운_세상_${회차}.hwpx

        return Regex.Replace(oldName, capture, replacement);
    }
}