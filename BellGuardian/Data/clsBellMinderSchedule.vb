' This class maps out a BellMinder schedule.
' This lets us deserialize a schedule file into
' this class, then serialize it and get the same file back.
Imports System.ComponentModel
Imports System.Xml.Serialization

<Serializable(), XmlRoot("MinderFile")>
Public Class clsBellMinderSchedule

    ' Stores information about our daylight savings times 
    Private Property _DST As TimeZoneInfo.AdjustmentRule = TimeZoneInfo.Local.GetAdjustmentRules.First

    <Description("Version of BellMinder this plan is for"), DisplayName("Minder Version")>
    Public Property MinderVersion As String = "1.0"

    <Description("What year this plan covers. Defaults to the current year"), DisplayName("Plan Year")>
    Public Property Year As Integer = Date.Now.Year

    <Description("The date Daylight Savings starts in the format of dd/MM/yyyy. Defaults to DST times for this year"), DisplayName("DST Start Date")>
    Public Property DaylightStartDate As String = New Date(Year, _DST.DaylightTransitionStart.Month, _DST.DaylightTransitionStart.Day).ToString("dd/MM/yyyy")

    <Description("The time Daylight Savings starts in the format hh:mm:ss tt. Defaults to DST times for this year"), DisplayName("DST Start Time")>
    Public Property DaylightStartTime As String = New Date(Year, _DST.DaylightTransitionStart.Month, _DST.DaylightTransitionStart.Day, _DST.DaylightTransitionStart.TimeOfDay.Hour, _DST.DaylightTransitionStart.TimeOfDay.Minute, _DST.DaylightTransitionStart.TimeOfDay.Second).ToString("hh:mm:ss tt")

    <Description("The date Daylight Savings ends in the format of dd/MM/yyyy. Defaults to DST times for this year"), DisplayName("DST End Date")>
    Public Property DaylightEndDate As String = New Date(Year, _DST.DaylightTransitionEnd.Month, _DST.DaylightTransitionEnd.Day).ToString("dd/MM/yyyy")

    <Description("The time Daylight Savings ends in the format hh:mm:ss tt. Defaults to DST times for this year"), DisplayName("DST End Time")>
    Public Property DaylightEndTime As String = New Date(Year, _DST.DaylightTransitionEnd.Month, _DST.DaylightTransitionEnd.Day, _DST.DaylightTransitionEnd.TimeOfDay.Hour, _DST.DaylightTransitionEnd.TimeOfDay.Minute, _DST.DaylightTransitionEnd.TimeOfDay.Second).ToString("hh:mm:ss tt")

    <Description("When this plan was last modified. Defaults to dd/MM/yyyy hh:mm:ss tt"), DisplayName("Last Saved Date")>
    Public Property SavedDateTime As String = Date.Now.ToString("dd/MM/yyyy hh:mm:ss tt")

    <XmlElement("RingPlan"), Description("A ring plan, which is a group of RingTimes grouped under a name, colour and plan number"), DisplayName("Ring Plans")>
    Public Property RingPlan() As BellMinderRingPlan()

    <XmlElement("Day"), Description("A day, which is a date and a plan number to use for that day"), DisplayName("Ring Day")>
    Public Property Day() As BellMinderDay()
End Class

<Serializable(), XmlRoot("RingPlan")>
Public Class BellMinderRingPlan
    <Description("The name of this ring plan"), DisplayName("Ring Plan Name")>
    Public Property PlanName As String

    <Description("The colour of this plan to be shown in the BellMinder software, and is a hex colour converted to an integer"), DisplayName("Ring Plan Colour")>
    Public Property PlanColour As Integer

    <Description("What plan number this is. This is used when constructing the packets that are sent to the controller"), DisplayName("Plan Number")>
    Public Property PlanNumber As String

    <XmlElement("RingTime"), Description("A group of times and durations that the bell will ring for"), DisplayName("Ring Times")>
    Public Property RingTime() As BellMinderRingTime()
End Class

<Serializable(), XmlRoot("RingTime")>
Public Class BellMinderRingTime
    <Description("The hour to ring, in 12 or 24 hour format, 0-12 or 0-24. If using 12 hour, set TimeAMPM"), DisplayName("Ring Time (Hour)")>
    Public Property TimeHH As Integer

    <Description("The minute to ring, 0-59"), DisplayName("Ring Time (Minute)")>
    Public Property TimeMM As Integer

    <Description("The second to ring, 0-50"), DisplayName("Ring Time (Seconds)")>
    Public Property TimeSS As Integer

    <Description("If using 12 hour time instead, set this to 0 for AM and 1 for PM")>
    Public Property TimeAMPM As Integer

    <Description("The duration to ring the bell in seconds. 253 = enable CrazyMan ring, 254 = enable PTI, 0 = disable PTI and CrazyMan, 255 = enable previous PTI or CrazyMan ring"), DisplayName("Bell Duration")>
    Public Property Duration As Integer

    <Description("A unique name for this bell. Not shown in the BellMinder software"), DisplayName("Bell Name")>
    Public Property Key As String
End Class

<Serializable(), XmlRoot("Day")>
Public Class BellMinderDay
    <Description("What day this applies to. In the format of YYYY/MMM/dd (e.g. 2023/Jan/01)"), DisplayName("Date")>
    Public Property DayDate() As String

    <Description("Which plan number to use"), DisplayName("Plan Number")>
    Public Property Plan() As Integer

    <Description("Which plan to use when the controller is in Alt mode"), DisplayName("Alt Plan Number")>
    Public Property AltPlan() As Integer

End Class




