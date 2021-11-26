using LemonMarkets.Models;

namespace LemonMarkets.Interfaces
{
    public interface ILemonApi
    {

        ConnectionInfo ConnectionInfo
        {
            get;
        }

        string ApiKey
        {
            get;
        }

        IOrdersRepo Orders
        {
            get;
        }

        ISpacesRepo Spaces
        {
            get;
        }

    }
}
