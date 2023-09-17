/*
    Chase CommonLib - LFInteractive LLC. 2021-2024
    CommonLib is a library of common functions and classes for .NET 6.0+.
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.CommonLib.FileSystem.Configuration;

namespace Test;

public class TestConfig : AppConfigBase<TestConfig>
{
    public string TestString { get; set; } = "Hello World!";
    public int TestInt { get; set; } = 123;
    public bool TestBool { get; set; } = true;
    public double TestDouble { get; set; } = 123.456;
    public float TestFloat { get; set; } = 123.456f;
    public decimal TestDecimal { get; set; } = 123.456m;
    public Guid TestGuid { get; set; } = Guid.NewGuid();
    public DateTime TestDateTime { get; set; } = DateTime.Now;
    public TimeSpan TestTimeSpan { get; set; } = TimeSpan.FromMinutes(5);
}