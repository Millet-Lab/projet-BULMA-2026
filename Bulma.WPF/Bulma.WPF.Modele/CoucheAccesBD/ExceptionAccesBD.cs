namespace Bulma.WPF.Modele.CoucheAccesBD
{
    [Serializable]
    public class ExceptionAccesBD : Exception
    {
        public ExceptionAccesBD(string message) : base(message)
        {

        }
        public ExceptionAccesBD(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
