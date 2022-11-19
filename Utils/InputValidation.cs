namespace GoBackendProjectStarter.Utils;

internal static class InputValidation
{
    internal static void GetUserInput(ref String? inputVar, String message )
    {
        do
        {
            Console.Write(message);
            inputVar = Console.ReadLine();
            inputVar = inputVar?.Trim();
            Thread.Sleep(500);
        }
        while(string.IsNullOrEmpty(inputVar));
    }
}