using Microsoft.Playwright;

namespace Page
{
    public class StudyGroupPage
    {
        private readonly IPage _page;

        public StudyGroupPage(IPage page)
        {
            _page = page;
        }

        private ILocator NameInput => _page.Locator("input[name='name']");
        private ILocator SubjectDropdown => _page.GetByRole(AriaRole.Combobox, new() { Name = "Subject" });
        private ILocator CreateButton => _page.GetByRole(AriaRole.Button, new() { Name = "Create" });
        private ILocator NotificationMessage => _page.Locator(".notification");

        private ILocator JoinButton => _page.GetByRole(AriaRole.Button, new() { Name = "Join" });
        private ILocator LeaveButton => _page.GetByRole(AriaRole.Button, new() { Name = "Leave" });

        private ILocator SortDropdown => _page.Locator("select.sort-dropdown");

        public async Task NavigateToAsync(string url)
        {
            await _page.GotoAsync(url);
        }

        public async Task SetGroupName(string name)
        {
            await NameInput.FillAsync(name);
        }

        public async Task SelectSubject(string subject)
        {
            await SubjectDropdown.ClickAsync();
            await _page.GetByRole(AriaRole.Option, new() { Name = subject }).ClickAsync();
        }

        public async Task ClickCreateButton()
        {
            await CreateButton.ClickAsync();
        }

        public async Task<string> GetNotificationMessage()
        {
            return await NotificationMessage.InnerTextAsync();
        }

        public async Task ClickJoinButton(string groupName)
        {
            var row = _page.Locator($"tr:has(td:text('{groupName}'))");
            await row.GetByRole(AriaRole.Button, new() { Name = "Join" }).ClickAsync();
        }

        public async Task ClickLeaveButton(string groupName)
        {
            var row = _page.Locator($"tr:has(td:text('{groupName}'))");
            await row.GetByRole(AriaRole.Button, new() { Name = "Leave" }).ClickAsync();
        }
        
        public async Task<int> GetGroupRowsCount()
        {
            return await _page.Locator(".study-group-row").CountAsync();
        }

        public async Task SelectSortOption(string sortOption)
        {
            await SortDropdown.SelectOptionAsync(new SelectOptionValue { Label = sortOption });
        }

        public async Task<string[]> GetColumnData(string columnClass)
        {
            var cells = _page.Locator($".study-group-row .{columnClass}");
            int count = await cells.CountAsync();
            var data = new string[count];
            for (int i = 0; i < count; i++)
            {
                data[i] = await cells.Nth(i).InnerTextAsync();
            }
            return data;
        }
    }
}
