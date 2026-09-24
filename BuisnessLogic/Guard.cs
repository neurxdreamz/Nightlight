namespace BuisnessLogic
{
    public static class Guard
    {
        /// <summary>
        /// метод проверяет условие и выбрасывает исключение с указанным сообщенимем еслии он не выполняется
        /// </summary>
        public static void Requires(bool condition, string message)
        {
            if (!condition)
            {
                throw new PreViolationException(message);
            }
        }
        /// <summary>
        /// пользовательское исключение для отделения контракта от других системных ошибок
        /// </summary>
        public class PreViolationException : Exception
        {
            public PreViolationException(string message) : base(message) { }
        }
    }
}
