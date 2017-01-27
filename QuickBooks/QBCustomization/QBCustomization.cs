using Intuit.Ipp.Core;

namespace QuickBooks.QBCustomization
{
    public static class QbCustomization
    {
        public static ServiceContext ApplyJsonSerilizationFormat(ServiceContext context)
        {
            context.IppConfiguration.Message.Request.SerializationFormat =
               Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
            context.IppConfiguration.Message.Response.SerializationFormat =
                Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
            return context;
        }
    }
}