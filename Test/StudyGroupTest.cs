using Microsoft.Playwright.NUnit;
using Page;

namespace Test
{
    public class StudyGroupTest : PageTest
    {
        private StudyGroupPage _studyGroupPage;

        [SetUp]
        public async Task Setup()
        {
            _studyGroupPage = new StudyGroupPage(Page);
            await _studyGroupPage.NavigateToAsync("https://students-app-test.com/studygroups");
        }

        [Test]
        public async Task Create_StudyGroup_With_Valid_Data()
        {
            string groupName = "Physics Pioneers";

            await _studyGroupPage.SetGroupName(groupName);
            await _studyGroupPage.SelectSubject(SubjectConstants.Physics);
            await _studyGroupPage.ClickCreateButton();

            string message = await _studyGroupPage.GetNotificationMessage();
            Assert.That(message.ToLower(), Does.Contain("success"));
        }

        [Test]
        public async Task Reject_Second_StudyGroup_For_Same_Subject()
        {
            string groupName1 = "Math Lords";
            string groupName2 = "Math Thinkers";

            await _studyGroupPage.SetGroupName(groupName1);
            await _studyGroupPage.SelectSubject(SubjectConstants.Math);
            await _studyGroupPage.ClickCreateButton();
            await _studyGroupPage.GetNotificationMessage(); // ignore

            await _studyGroupPage.SetGroupName(groupName2);
            await _studyGroupPage.SelectSubject(SubjectConstants.Math);
            await _studyGroupPage.ClickCreateButton();

            string error = await _studyGroupPage.GetNotificationMessage();
            Assert.That(error.ToLower(), Does.Contain("only one").Or.Contain("already exists"));
        }

        [Test]
        public async Task Reject_StudyGroup_Name_Too_Short_Or_Too_Long()
        {
            string shortName = "Grp1";
            string longName = "Advanced QuantumMechanicsLegend";

            await _studyGroupPage.SetGroupName(shortName);
            await _studyGroupPage.SelectSubject(SubjectConstants.Chemistry);
            await _studyGroupPage.ClickCreateButton();
            string errorShort = await _studyGroupPage.GetNotificationMessage();
            Assert.That(errorShort, Does.Contain("name must be between").Or.Contain("invalid name"));

            await _studyGroupPage.SetGroupName(longName);
            await _studyGroupPage.SelectSubject(SubjectConstants.Chemistry);
            await _studyGroupPage.ClickCreateButton();
            string errorLong = await _studyGroupPage.GetNotificationMessage();
            Assert.That(errorLong, Does.Contain("name must be between").Or.Contain("invalid name"));
        }

        [Test]
        public async Task StudyGroup_Creation_Records_Timestamp()
        {
            string groupName = "Test Group";

            await _studyGroupPage.SetGroupName(groupName);
            await _studyGroupPage.SelectSubject(SubjectConstants.Physics);
            await _studyGroupPage.ClickCreateButton();
            await _studyGroupPage.GetNotificationMessage();

            string createdAtText = await Page.Locator($".study-group-row:has-text(\'{groupName}\') >> .created-at").InnerTextAsync();
            Assert.IsNotNull(createdAtText);
            Assert.That(DateTime.TryParse(createdAtText, out _), Is.True, "Timestamp is not in valid date format");
        }

        [Test]
        public async Task User_Can_Join_Existing_StudyGroup()
        {
            string groupName = "Physics Grupetto";
            await _studyGroupPage.ClickJoinButton(groupName);

            string message = await _studyGroupPage.GetNotificationMessage();
            Assert.That(message.ToLower(), Does.Contain("joined"));
        }

        [Test]
        public async Task User_Can_Leave_Joined_StudyGroup()
        {
            string groupName = "Physics Grupetto";
            await _studyGroupPage.ClickLeaveButton(groupName);

            string message = await _studyGroupPage.GetNotificationMessage();
            Assert.That(message.ToLower(), Does.Contain("left").Or.Contain("success"));
        }

        [Test]
        public async Task StudyGroup_List_Displays_All_Groups()
        {
            int count = await _studyGroupPage.GetGroupRowsCount();
            Assert.That(count, Is.GreaterThan(0), "No study groups were displayed");
        }

        [Test]
        public async Task StudyGroups_Can_Be_Filtered_By_Subject()
        {
            // Set 'Chemistry' in the Study Groups search bar or filter
            await _studyGroupPage.SelectSubject("Chemistry");

            var filteredRows = Page.Locator(".study-group-row");
            int count = await filteredRows.CountAsync();
            for (int i = 0; i < count; i++)
            {
                string subjectText = await filteredRows.Nth(i).Locator(".subject").InnerTextAsync();
                Assert.That(subjectText.ToLower(), Is.EqualTo("chemistry"));
            }
        }

        [Test]
        public async Task StudyGroups_Sorted_By_Newest_First()
        {
            await _studyGroupPage.SelectSortOption("Newest First");

            // Use GetColumnData to get all created-at values
            string[] createdAtArray = await _studyGroupPage.GetColumnData("created-at");

            List<DateTime> creationDates = [];
            foreach (string dateText in createdAtArray)
            {
                if (DateTime.TryParse(dateText, out DateTime date))
                    creationDates.Add(date);
            }

             // Check if the list is already sorted in descending order (newest first)
            bool isSorted = true;
            for (int i = 1; i < creationDates.Count; i++)
            {
                if (creationDates[i] > creationDates[i - 1])
                {
                    isSorted = false;
                    break;
                }
            }
            Assert.That(isSorted, Is.True, "Study groups are not sorted by newest first.");
        }

        [Test]
        public async Task StudyGroups_Sorted_By_Oldest_First()
        {
            await _studyGroupPage.SelectSortOption("Oldest First");

            // Use GetColumnData to get all created-at values
            string[] createdAtArray = await _studyGroupPage.GetColumnData("created-at");

            List<DateTime> creationDates = [];
            foreach (string dateText in createdAtArray)
            {
                if (DateTime.TryParse(dateText, out DateTime date))
                    creationDates.Add(date);
            }

            // Check if the list is already sorted in ascending order (oldest first)
            bool isSorted = true;
            for (int i = 1; i < creationDates.Count; i++)
            {
                if (creationDates[i] < creationDates[i - 1])
                {
                    isSorted = false;
                    break;
                }
            }
            Assert.That(isSorted, Is.True, "Study groups are not sorted by oldest first.");
        }
    }
}