namespace EnterpriseBase.Employees.Dto
{
    public class GetEmployeeForEditOutput
    {
        public CreateEmployeeEditDto Employee { get; set; }
        public string EmployeeTypeName { get; set; }
        public string BranchName { get; set; }
        public string UserName { get; set; }
    }
}