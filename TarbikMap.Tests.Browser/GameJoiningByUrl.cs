namespace TarbikMap.Tests.Browser
{
    using System.Linq;
    using Xunit;

    public class GameJoiningByUrl
    {
        [Fact]
        public void Test()
        {
            var driver_a_1 = new DriverWrapper(0, 0);
            var driver_b_1 = new DriverWrapper(1, 0);
            var driver_c_1 = new DriverWrapper(2, 0);
            var driver_o_1 = new DriverWrapper(3, 0);

            try
            {
                driver_a_1.GoToPath(string.Empty);
                driver_a_1.ClickSpanByText("Custom");
                driver_a_1.FillInputByPlaceholder("Enter Your Name", "JoinedPlayerA");
                driver_a_1.ClickButtonByText("Join");

                string gameId = driver_a_1.GetUrl().Split("/").Last();

                driver_b_1.GoToPath("game/" + gameId);
                driver_c_1.GoToPath("game/" + gameId);
                driver_o_1.GoToPath("game/" + gameId);

                driver_b_1.FillInputByPlaceholder("Enter Your Name", "JoinedPlayerB");
                driver_b_1.ClickButtonByText("Join");

                driver_c_1.FillInputByPlaceholder("Enter Your Name", "JoinedPlayerC");
                driver_c_1.ClickButtonByText("Join");

                SeesJoinedPlayers(driver_a_1);
                SeesJoinedPlayers(driver_b_1);
                SeesJoinedPlayers(driver_c_1);
                SeesJoinedPlayers(driver_o_1);
            }
            finally
            {
                driver_a_1.Dispose();
                driver_b_1.Dispose();
                driver_c_1.Dispose();
                driver_o_1.Dispose();
            }
        }

        private static void SeesJoinedPlayers(DriverWrapper driver)
        {
            driver.WaitForText("JoinedPlayerA");
            driver.WaitForText("JoinedPlayerB");
            driver.WaitForText("JoinedPlayerC");
        }
    }
}
