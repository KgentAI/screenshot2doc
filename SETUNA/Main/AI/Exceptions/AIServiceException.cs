using System;

namespace SETUNA.Main.AI.Exceptions
{
    /// <summary>
    /// Base exception for all AI service errors
    /// </summary>
    public class AIServiceException : Exception
    {
        public AIServiceException() : base()
        {
        }

        public AIServiceException(string message) : base(message)
        {
        }

        public AIServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception for AI service configuration errors
    /// </summary>
    public class AIServiceConfigurationException : AIServiceException
    {
        public AIServiceConfigurationException() : base()
        {
        }

        public AIServiceConfigurationException(string message) : base(message)
        {
        }

        public AIServiceConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception for AI service timeout errors
    /// </summary>
    public class AIServiceTimeoutException : AIServiceException
    {
        public AIServiceTimeoutException() : base()
        {
        }

        public AIServiceTimeoutException(string message) : base(message)
        {
        }

        public AIServiceTimeoutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception for AI service authentication errors
    /// </summary>
    public class AIServiceAuthenticationException : AIServiceException
    {
        public AIServiceAuthenticationException() : base()
        {
        }

        public AIServiceAuthenticationException(string message) : base(message)
        {
        }

        public AIServiceAuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception for AI service network errors
    /// </summary>
    public class AIServiceNetworkException : AIServiceException
    {
        public AIServiceNetworkException() : base()
        {
        }

        public AIServiceNetworkException(string message) : base(message)
        {
        }

        public AIServiceNetworkException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
