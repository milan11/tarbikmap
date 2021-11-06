namespace TarbikMap.Storage
{
    internal class TaskCreationDetails
    {
        public TaskCreationDetails(double queriedLat, double queriedLon)
        {
            this.QueriedLat = queriedLat;
            this.QueriedLon = queriedLon;
        }

        public double QueriedLat { get; private set; }

        public double QueriedLon { get; private set; }
    }
}
