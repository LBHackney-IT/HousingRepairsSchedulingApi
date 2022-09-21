namespace HousingRepairsSchedulingApi.Exceptions
{
    using System;

    public class DrsException : Exception
    {
        public DrsException() { }
        public DrsException(string errorMessage) => this.ErrorMessage = errorMessage;
        public string ErrorMessage { get; set; }
    }
}
