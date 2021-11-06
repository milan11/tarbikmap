namespace TarbikMap
{
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using TarbikMap.Controllers;

    internal class CustomControllerFeatureProvider : ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            return typeInfo.AsType() == typeof(GameController);
        }
    }
}
