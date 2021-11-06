namespace TarbikMap.Tests.Browser
{
    using Xunit;

    public class Home
    {
        [Fact]
        public void Test()
        {
            var driver_a_1 = new DriverWrapper(0, 0);

            try
            {
                driver_a_1.GoToPath(string.Empty);
                TestNewGame(driver_a_1);

                driver_a_1.GoToPath("/join");
                TestJoinGame(driver_a_1);

                driver_a_1.GoToPath("/about");
                TestAbout(driver_a_1);

                driver_a_1.ClickLinkByText("New Game");
                TestNewGame(driver_a_1);

                driver_a_1.ClickLinkByText("Join Game");
                TestJoinGame(driver_a_1);

                driver_a_1.ClickLinkByText("About");
                TestAbout(driver_a_1);
            }
            finally
            {
                driver_a_1.Dispose();
            }
        }

        private static void TestNewGame(DriverWrapper driver)
        {
            driver.WaitForText("Choose any area");
        }

        private static void TestJoinGame(DriverWrapper driver)
        {
            driver.FillInputByPlaceholder("Enter Game Code", "AAAA");
        }

        private static void TestAbout(DriverWrapper driver)
        {
            driver.WaitForText("babel/runtime");
            driver.ClickLinkByText("babel/runtime");
            driver.WaitForText("and other contributors");
        }
    }
}
