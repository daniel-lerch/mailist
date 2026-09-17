using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ChurchTools.Model;

public class GlobalPermissions
{
    [JsonPropertyName("churchcal")]
    public ChurchCalPermissions? ChurchCal { get; set; }

    [JsonPropertyName("churchcheckin")]
    public ChurchCheckinPermissions? ChurchCheckin { get; set; }

    [JsonPropertyName("churchcore")]
    public required ChurchCorePermissions ChurchCore { get; set; }

    [JsonPropertyName("churchdb")]
    public ChurchDbPermissions? ChurchDb { get; set; }

    [JsonPropertyName("churchgroup")]
    public ChurchGroupPermissions? ChurchGroup { get; set; }

    [JsonPropertyName("churchreport")]
    public ChurchReportPermissions? ChurchReport { get; set; }

    [JsonPropertyName("churchresource")]
    public ChurchResourcePermissions? ChurchResource { get; set; }

    [JsonPropertyName("churchservice")]
    public ChurchServicePermissions? ChurchService { get; set; }

    [JsonPropertyName("churchsync")]
    public ChurchSyncPermissions? ChurchSync { get; set; }

    [JsonPropertyName("churchwiki")]
    public ChurchWikiPermissions? ChurchWiki { get; set; }

    [JsonPropertyName("finance")]
    public FinancePermissions? Finance { get; set; }

    [JsonPropertyName("mailist")]
    public ExtensionPermissions? Mailist { get; set; }

    [JsonPropertyName("post")]
    public PostPermissions? Post { get; set; }


    public class ChurchCalPermissions
    {
        [JsonPropertyName("admin church category")]
        public required bool AdminChurchCategory { get; set; }

        [JsonPropertyName("admin group category")]
        public required bool AdminGroupCategory { get; set; }

        [JsonPropertyName("admin personal category")]
        public required bool AdminPersonalCategory { get; set; }

        [JsonPropertyName("assistance mode")]
        public required bool AssistanceMode { get; set; }

        [JsonPropertyName("create group category")]
        public required bool CreateGroupCategory { get; set; }

        [JsonPropertyName("create personal category")]
        public required bool CreatePersonalCategory { get; set; }

        [JsonPropertyName("edit calendar entry template")]
        public required List<int> EditCalendarEntryTemplate { get; set; }

        [JsonPropertyName("edit category")]
        public required List<int> EditCategory { get; set; }

        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view category")]
        public required List<int> ViewCategory { get; set; }
    }

    public class ChurchCheckinPermissions
    {
        [JsonPropertyName("create person")]
        public required bool CreatePerson { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }

        [JsonPropertyName("view")]
        public required bool View { get; set; }
    }

    public class ChurchCorePermissions
    {
        [JsonPropertyName("administer church html templates")]
        public required bool AdministerChurchHtmlTemplates { get; set; }

        //[JsonPropertyName("administer custom modules")]
        //public required bool AdministerCustomModules { get; set; }

        [JsonPropertyName("administer persons")]
        public required bool AdministerPersons { get; set; }

        [JsonPropertyName("administer settings")]
        public required bool AdministerSettings { get; set; }

        [JsonPropertyName("edit languages")]
        public required List<int> EditLanguages { get; set; }

        [JsonPropertyName("edit public profiles")]
        public required bool EditPublicProfiles { get; set; }

        [JsonPropertyName("edit translations masterdata")]
        public required bool EditTranslationsMasterdata { get; set; }

        [JsonPropertyName("edit website releases")]
        public required bool EditWebsiteReleases { get; set; }

        [JsonPropertyName("edit website staff")]
        public required bool EditWebsiteStaff { get; set; }

        [JsonPropertyName("invite persons")]
        public required bool InvitePersons { get; set; }

        [JsonPropertyName("login to external system")]
        public required List<int> LoginToExternalSystem { get; set; }

        [JsonPropertyName("simulate persons")]
        public required bool SimulatePersons { get; set; }

        [JsonPropertyName("use church html templates")]
        public required List<int> UseChurchHtmlTemplates { get; set; }

        //[JsonPropertyName("use churchquery")]
        //public required bool UseChurchquery { get; set; }

        [JsonPropertyName("view logfile")]
        public required bool ViewLogfile { get; set; }

        [JsonPropertyName("view website")]
        public required bool ViewWebsite { get; set; }
    }

    public class ChurchDbPermissions
    {
        [JsonPropertyName("administer global filters")]
        public required bool AdministerGlobalFilters { get; set; }

        [JsonPropertyName("administer groups")]
        public required bool AdministerGroups { get; set; }

        [JsonPropertyName("complex filter")]
        public required bool ComplexFilter { get; set; }

        [JsonPropertyName("create groups of grouptype")]
        public required List<int> CreateGroupsOfGrouptype { get; set; }

        [JsonPropertyName("create person")]
        public required bool CreatePerson { get; set; }

        [JsonPropertyName("create print labels")]
        public required bool CreatePrintLabels { get; set; }

        [JsonPropertyName("delete group")]
        public required List<int> DeleteGroup { get; set; }

        [JsonPropertyName("delete groups of grouptype")]
        public required List<int> DeleteGroupsOfGrouptype { get; set; }

        [JsonPropertyName("delete persons")]
        public required bool DeletePersons { get; set; }

        [JsonPropertyName("edit bulkletter")]
        public required bool EditBulkletter { get; set; }

        [JsonPropertyName("edit group")]
        public required List<int> EditGroup { get; set; }

        [JsonPropertyName("edit group memberships")]
        public required bool EditGroupMemberships { get; set; }

        [JsonPropertyName("edit group memberships of group")]
        public required List<int> EditGroupMembershipsOfGroup { get; set; }

        [JsonPropertyName("edit group memberships of grouptype")]
        public required List<int> EditGroupMembershipsOfGrouptype { get; set; }

        [JsonPropertyName("edit groups of grouptype")]
        public required List<int> EditGroupsOfGrouptype { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }

        [JsonPropertyName("edit relations")]
        public required bool EditRelations { get; set; }

        [JsonPropertyName("export data")]
        public required bool ExportData { get; set; }

        [JsonPropertyName("push/pull archive")]
        public required bool PushPullArchive { get; set; }

        [JsonPropertyName("security level edit own data")]
        public required List<int> SecurityLevelEditOwnData { get; set; }

        [JsonPropertyName("security level group")]
        public required List<int> SecurityLevelGroup { get; set; }

        [JsonPropertyName("security level person")]
        public required List<int> SecurityLevelPerson { get; set; }

        [JsonPropertyName("security level view own data")]
        public required List<int> SecurityLevelViewOwnData { get; set; }

        [JsonPropertyName("send sms")]
        public required bool SendSms { get; set; }

        [JsonPropertyName("view")]
        public required bool View { get; set; }

        /// <summary>
        /// List of department IDs from which all people can be seen
        /// </summary>
        [JsonPropertyName("view alldata")]
        public required List<int> ViewAllData { get; set; }

        [JsonPropertyName("view archive")]
        public required bool ViewArchive { get; set; }

        [JsonPropertyName("view comments")]
        public required List<int> ViewComments { get; set; }

        [JsonPropertyName("view group")]
        public required List<int> ViewGroup { get; set; }

        [JsonPropertyName("view groups of grouptype")]
        public required List<int> ViewGroupsOfGrouptype { get; set; }

        [JsonPropertyName("view history")]
        public required bool ViewHistory { get; set; }

        [JsonPropertyName("view memberliste")]
        public required bool ViewMemberListe { get; set; }

        [JsonPropertyName("view station")]
        public required List<int> ViewStation { get; set; }

        [JsonPropertyName("view statistics")]
        public required bool ViewStatistics { get; set; }

        [JsonPropertyName("view tags")]
        public required bool ViewTags { get; set; }

        [JsonPropertyName("write access")]
        public required bool WriteAccess { get; set; }
    }

    public class ChurchGroupPermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }
    }

    public class ChurchReportPermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view query")]
        public required List<int> ViewQuery { get; set; }
    }

    public class ChurchResourcePermissions
    {
        [JsonPropertyName("administer bookings")]
        public required List<int> AdministerBookings { get; set; }

        [JsonPropertyName("assistance mode")]
        public required bool AssistanceMode { get; set; }

        [JsonPropertyName("create bookings")]
        public required List<int> CreateBookings { get; set; }

        [JsonPropertyName("create virtual bookings")]
        public required bool CreateVirtualBookings { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }

        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view resource")]
        public required List<int> ViewResource { get; set; }
    }

    public class ChurchServicePermissions
    {
        [JsonPropertyName("edit agenda")]
        public required List<int> EditAgenda { get; set; }

        [JsonPropertyName("edit agenda templates")]
        public required List<int> EditAgendaTemplates { get; set; }

        [JsonPropertyName("edit events")]
        public required List<int> EditEvents { get; set; }

        [JsonPropertyName("edit fact")]
        public required List<int> EditFact { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }

        [JsonPropertyName("edit servicegroup")]
        public required List<int> EditServiceGroup { get; set; }

        [JsonPropertyName("edit songcategory")]
        public required List<int> EditSongCategory { get; set; }

        [JsonPropertyName("edit template")]
        public required bool EditTemplate { get; set; }

        [JsonPropertyName("export facts")]
        public required bool ExportFacts { get; set; }

        [JsonPropertyName("manage absent")]
        public required bool ManageAbsent { get; set; }

        [JsonPropertyName("use ccli")]
        public required bool UseCcli { get; set; }

        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view agenda")]
        public required List<int> ViewAgenda { get; set; }

        [JsonPropertyName("view events")]
        public required List<int> ViewEvents { get; set; }

        [JsonPropertyName("view fact")]
        public required List<int> ViewFact { get; set; }

        [JsonPropertyName("view history")]
        public required bool ViewHistory { get; set; }

        [JsonPropertyName("view servicegroup")]
        public required List<int> ViewServiceGroup { get; set; }

        [JsonPropertyName("view song statistics")]
        public required bool ViewSongStatistics { get; set; }

        [JsonPropertyName("view songcategory")]
        public required List<int> ViewSongCategory { get; set; }
    }

    public class ChurchSyncPermissions
    {
        [JsonPropertyName("view")]
        public required bool View { get; set; }
    }

    public class ChurchWikiPermissions
    {
        [JsonPropertyName("edit category")]
        public required List<int> EditCategory { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }

        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view category")]
        public required List<int> ViewCategory { get; set; }
    }

    public class FinancePermissions
    {
        [JsonPropertyName("edit accounting period")]
        public required List<int> EditAccountingPeriod { get; set; }

        [JsonPropertyName("edit masterdata")]
        public required bool EditMasterdata { get; set; }

        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view accounting period")]
        public required List<int> ViewAccountingPeriod { get; set; }
    }

    public class ExtensionPermissions
    {
        [JsonPropertyName("create custom category")]
        public required bool CreateCustomCategory { get; set; }

        [JsonPropertyName("create custom data")]
        public required List<int> CreateCustomData { get; set; }

        [JsonPropertyName("delete custom category")]
        public required List<int> DeleteCustomCategory { get; set; }

        [JsonPropertyName("delete custom data")]
        public required List<int> DeleteCustomData { get; set; }

        [JsonPropertyName("edit custom category")]
        public required List<int> EditCustomCategory { get; set; }

        [JsonPropertyName("edit custom data")]
        public required List<int> EditCustomData { get; set; }

        [JsonPropertyName("view")]
        public required bool View { get; set; }

        [JsonPropertyName("view custom category")]
        public required List<int> ViewCustomCategory { get; set; }

        [JsonPropertyName("view custom data")]
        public required List<int> ViewCustomData { get; set; }
    }

    public class PostPermissions
    {
        [JsonPropertyName("moderate posts")]
        public required bool ModeratePosts { get; set; }
    }
}
