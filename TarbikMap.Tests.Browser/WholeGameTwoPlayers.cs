namespace TarbikMap.Tests.Browser
{
    using System.Linq;
    using System.Threading;
    using Xunit;

    public class WholeGameTwoPlayers
    {
        [Fact]
        public void Test()
        {
            var driver_a_1 = new DriverWrapper(0, 0);
            var driver_b_1 = new DriverWrapper(1, 0);

            try
            {
                driver_a_1.GoToPath(string.Empty);
                driver_a_1.ClickSpanByText("Custom");
                driver_a_1.FillInputByPlaceholder("Enter Your Name", "JoinedPlayerA");
                driver_a_1.ClickButtonByText("Join");

                driver_a_1.ClickLinkByText("Type");
                driver_a_1.ClickLinkByText("Squares");
                driver_a_1.ClickButtonByText("Select");

                driver_a_1.ClickLinkByText("Area");
                driver_a_1.ClickButtonByText("Select");

                driver_a_1.ClickLinkByText("Settings");
                driver_a_1.ClickButtonByText("Save");

                string gameId = driver_a_1.GetUrl().Split("/").Last();

                driver_b_1.GoToPath(string.Empty);
                driver_b_1.ClickLinkByText("Join Game");
                driver_b_1.FillInputByPlaceholder("Enter Game Code", gameId);
                driver_b_1.FillInputByPlaceholder("Enter Your Name", "JoinedPlayerB");
                driver_b_1.ClickButtonByText("Join");

                driver_a_1.ClickButtonByText("Start Game");
                driver_a_1.ClickButtonByText("Confirm");

                for (int i = 0; i < 5; ++i)
                {
                    driver_a_1.ClickButtonByText("Go to Map");
                    driver_a_1.ClickCoordinates(300, 400);
                    driver_a_1.ClickButtonByText("Submit Answer");

                    driver_b_1.ClickButtonByText("Go to Map");
                    driver_b_1.ClickCoordinates(200, 300);
                    driver_b_1.ClickButtonByText("Submit Answer");

                    driver_a_1.ClickButtonByText("Continue");
                    driver_b_1.ClickButtonByText("Continue");
                    Thread.Sleep(2000);
                }

                driver_a_1.ClickButtonByText("Create Next Game");
                driver_b_1.ClickButtonByText("Go to Next Game");
            }
            finally
            {
                driver_a_1.Dispose();
                driver_b_1.Dispose();
            }
        }
    }
}
