namespace TicketsBooking.Core.Validators
{
    public static class CpfValidator
    {
        public static bool IsValid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            // Remove máscara: pontos e hífen
            cpf = cpf.Replace(".", "").Replace("-", "").Trim();

            if (cpf.Length != 11)
                return false;

            // Rejeita sequências inválidas conhecidas (ex: 111.111.111-11)
            if (cpf.Distinct().Count() == 1)
                return false;

            // Valida primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += int.Parse(cpf[i].ToString()) * (10 - i);

            int remainder = (sum * 10) % 11;
            if (remainder == 10 || remainder == 11) remainder = 0;
            if (remainder != int.Parse(cpf[9].ToString()))
                return false;

            // Valida segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += int.Parse(cpf[i].ToString()) * (11 - i);

            remainder = (sum * 10) % 11;
            if (remainder == 10 || remainder == 11) remainder = 0;
            if (remainder != int.Parse(cpf[10].ToString()))
                return false;

            return true;
        }
    }
}
