
using src.Shared.Global;

namespace UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.True(true, "This test should always pass.");

            // Example of a test that checks if the verification code is generated correctly
            var code = GlobalConstansts.GenerateVerificationCode();
            Assert.NotNull(code);
            Assert.Matches(@"^\d{7}$", code); 
            Assert.Equal(7, code.Length);
        }
    }
}