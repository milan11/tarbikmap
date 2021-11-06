namespace TarbikMap.Tests.Browser
{
    using Xunit;

    public class GameStarting
    {
        [Fact]
        public void Test()
        {
            var driver_a_1 = new DriverWrapper(0, 0);

            try
            {
                driver_a_1.GoToPath(string.Empty);
                driver_a_1.ClickSpanByText("Custom");
                driver_a_1.FillInputByPlaceholder("Enter Your Name", "a");
                driver_a_1.ClickButtonByText("Join");

                driver_a_1.ClickLinkByText("Type");
                driver_a_1.ClickLinkByText("Squares");
                driver_a_1.ClickButtonByText("Select");

                driver_a_1.ClickLinkByText("Area");
                driver_a_1.ClickButtonByText("Select");

                driver_a_1.ClickButtonByText("Start Game");
                driver_a_1.ClickButtonByText("Confirm");
            }
            finally
            {
                driver_a_1.Dispose();
            }
        }
    }
}