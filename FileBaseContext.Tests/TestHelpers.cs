using FileBaseContext.Tests.Csv;

namespace FileBaseContext.Tests;

public class TestHelpers
{
    public static void AssertString(string expected, string actual)
    {
        if (expected == null || actual == null)
            Assert.Fail("Expected and/or Actual string is null.");

        for (int i = 0; i < expected.Length; i++)
        {
            if (expected[i] != actual[i])
            {
                Assert.Fail($"{expected[i]} != {actual[i]} at {i}. \r\nExpected:{SimplePositiveTestsForCsvDatabase.GetSubstring(expected, i, 5)}\r\nActual  :{SimplePositiveTestsForCsvDatabase.GetSubstring(actual, i, 5)}");
            }
        }
    }
}