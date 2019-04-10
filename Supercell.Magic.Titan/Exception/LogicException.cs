namespace Supercell.Magic.Titan.Exception
{
    using System;

    public class LogicException : Exception
    {
        public LogicException()
        {
        }

        public LogicException(string message) : base(message)
        {
        }
    }
}