Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Net.Http
Imports System.Xml.Serialization

' This class is basically the BellMinder app condensed into a VB.NET class
Public Class clsBellMinder

    Dim _SerialPort As clsSerial = Nothing ' The serial port class which lets us talk to the BellMinder box
    Dim _YearDays As Integer = 32 * 14 ' How many days in the "year" are there? Bellminder works on a 14 month, 32 day thing.

    Public Property DiagnosticMode As Boolean = False ' If we're in diagnostic mode or not
    Public Property DateMode As BellMinderDateMode = BellMinderDateMode.DDMMYY ' What date mode we're in (MM/DD/YY or DD/MM/YY)
    Public Property TimeMode As BellMinderTimeMode = BellMinderTimeMode.TwelveHour ' What time format? 12 or 24 hour

    <Description("Contains Parent Teacher Interview ring settings"), DisplayName("Parent Teacher Interview Ring")>
    Public Class PTI
        <Description("How long to ring for"), DefaultValue(5)>
        Public Property RingDuration As Integer = 5

        <Description("How long to wait until ringing again (in seconds)"), DefaultValue(600)>
        Public Property RingPause As Integer = 600
    End Class

    <Description("Contains CrazyMan (emergency) ring settings"), DisplayName("CrazyMan Ring")>
    Public Class CrazyMan
        <Description("How long to ring for"), DefaultValue(2)>
        Public Property RingDuration As Integer = 2

        <Description("How long to wait until ringing again (in seconds)"), DefaultValue(2)>
        Public Property RingPause As Integer = 2
    End Class

    <Description("Date display format on the BellMinder controller"), DefaultValue(2)>
    Public Enum BellMinderDateMode
        MMDDYY = 1
        DDMMYY = 2
    End Enum

    <Description("Time display format on the BellMinder controller"), DefaultValue(12)>
    Public Enum BellMinderTimeMode
        <Description("12 Hour Time")>
        TwelveHour = 12
        <Description("24 Hour Time")>
        TwentyFourHour = 24
    End Enum

    Public Sub New(Port As String)
        _SerialPort = New clsSerial(Port, 4800)
        _SerialPort.Open()
    End Sub

    <Description("Enables or disables diagnostics on the Bellminder controller"), Category("Diagnostics"), DisplayName("Set Diagnostic Mode"), DefaultValue(False)>
    Public Function SetDiagnosticMode(value As Boolean) As Boolean
        Dim command As Byte() = Nothing
        If value = True Then
            command = {
                &H1B,
                &H48
            }
        Else
            command = {
                &H1B,
                &H49
            }
        End If

        _DiagnosticMode = value
        _SerialPort.Send(command)

        Return DiagnosticMode
    End Function

    <Description("The time mode used on the BellMinder controller"), Category("Settings"), DisplayName("Set Time Mode"), DefaultValue(BellMinderTimeMode.TwentyFourHour)>
    Public Function SetTimeMode(mode As BellMinderTimeMode)
        Dim command As Byte() = Nothing
        Select Case mode
            Case BellMinderTimeMode.TwentyFourHour
                command = {
                    &H1B,
                    &H4C
                }
            Case BellMinderTimeMode.TwelveHour
                command = {
                    &H1B,
                    &H4D
                }
        End Select

        TimeMode = mode
        _SerialPort.Send(command)

        Return TimeMode
    End Function


    <Description("The date mode used on the BellMinder controller"), Category("Settings"), DisplayName("Date Mode"), DefaultValue(BellMinderDateMode.DDMMYY)>
    Public Function SetDateMode(mode As BellMinderDateMode)
        Dim command As Byte() = Nothing
        Select Case mode
            Case BellMinderDateMode.MMDDYY
                command = {
                        &H1B,
                        &H4A
                    }
            Case BellMinderDateMode.DDMMYY
                command = {
                        &H1B,
                        &H4B
                    }
        End Select

        DateMode = mode
        _SerialPort.Send(command)
        Return DateMode

    End Function

    <Description("Attempts to get the date and time from the controller"), Category("Methods"), DisplayName("Get Time and Date")>
    Public Function GetControllerDateTime() As Date

        ' Prepare the command 
        Dim command As Byte() = {
            &H1B,
            &H58
        }

        ' Send the data, then wait for the response
        Dim response As String = _SerialPort.Send(command, True)

        ' The response will be in the format of YYMMDD-HHMMSS Use ParseExact to do it
        Dim d As Date = DateTime.ParseExact(response, "yyMMdd-HHmmss", CultureInfo.InvariantCulture)

        Return d

    End Function

    <Description("Set the time and date on the controller"), Category("Methods"), DisplayName("Set Time and Date")>
    Public Sub SetTime(setDate As Date)

        ' Convert the year into a two-digit year. Easiest way is to subtract 2000 from the current year. 
        Dim twoYearDate As Integer = setDate.Year - 2000

        ' Prepare the command 
        Dim command As Byte() = {
            &H1B,
            &H41
        }

        Dim command2 As Byte() = {
            BitConverter.GetBytes(setDate.Hour).First,
            BitConverter.GetBytes(setDate.Minute).First,
            BitConverter.GetBytes(setDate.Second).First,
            BitConverter.GetBytes(twoYearDate).First,
            BitConverter.GetBytes(setDate.Month).First,
            BitConverter.GetBytes(setDate.Day).First
        }

        ' We need to send the two commands separately.
        _SerialPort.Send(command)
        _SerialPort.Send(command2)

    End Sub

    <Description("Load a BellMinder schedule"), Category("Methods"), DisplayName("Load Schedule")>
    Public Function LoadSchedule(file As String)
        Dim serializer As New XmlSerializer(GetType(clsBellMinderSchedule))
        Dim minder As New clsBellMinderSchedule
        Using reader As New StreamReader(file)
            minder = DirectCast(serializer.Deserialize(reader), clsBellMinderSchedule)
        End Using

        Return minder
    End Function

    <Description("Save a BellMinder schedule to a file"), Category("Methods"), DisplayName("Save Schedule")>
    Public Sub SaveSchedule(schedule As clsBellMinderSchedule, filename As String)
        Dim serializer As New XmlSerializer(GetType(clsBellMinderSchedule))
        Using writer As New StreamWriter(filename)
            serializer.Serialize(writer, schedule)
        End Using
    End Sub

    <Description("Sends the schedule to the controller"), Category("Methods"), DisplayName("Send Schedule")>
    Public Sub SendSchedule(schedule As clsBellMinderSchedule)
        ' This is gonna be looooong!

        ' The controller expects you to send 14 ring plans with 40 rings in each plan.
        ' If you don't have 14 ring plans or 40 rings, you need to pad the remaining bytes with 0x30
        Dim RingPlanCount As Integer = 14 * 40

        ' First we convert the DST date and time to a general date / time object 
        Dim dstStartDate As Date = Date.ParseExact($"{schedule.DaylightStartDate} {schedule.DaylightStartTime}", "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)

        ' Then we do the same with the DST end time
        Dim dstEndDate As Date = Date.ParseExact($"{schedule.DaylightEndDate} {schedule.DaylightEndTime}", "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)

        ' Now we start putting everything together

        ' And we create our DST start and end packet
        Dim dstStartBytes As Byte() = {
            BitConverter.GetBytes(dstStartDate.Day).First,
            BitConverter.GetBytes(dstStartDate.Month).First,
            BitConverter.GetBytes(dstStartDate.Hour).First,
            BitConverter.GetBytes(dstStartDate.Minute).First,
            BitConverter.GetBytes(dstStartDate.Second).First
        }

        Dim dstEndBytes As Byte() = {
            BitConverter.GetBytes(dstEndDate.Day).First,
            BitConverter.GetBytes(dstEndDate.Month).First,
            BitConverter.GetBytes(dstEndDate.Hour).First,
            BitConverter.GetBytes(dstEndDate.Minute).First,
            BitConverter.GetBytes(dstEndDate.Second).First
        }

        ' Now we get how many ring plans we're going to send. I guess in theory
        ' you could send just 1 ring plan with 40 rings in it, but the BellMinder
        ' software seems to send 14 ring plans, so we'll follow suit.
        Dim ringCount() As Byte = {
            BitConverter.GetBytes(schedule.RingPlan.Length).First
        }

        ' This is an array of bytes that will make up our ring plan.
        Dim ringPlans() As Byte = {}

        ' Loop through each RingPlan.
        For plan = 0 To schedule.RingPlan.Length - 1

            ' If the ring plan is the default "no time set" plan, then skip.
            If schedule.RingPlan(plan).PlanName = "No Time Set" Then
                Continue For
            End If

            ' Make a single ringPlan byte array. Fill it with 160 bytes of 0x30
            Dim ringPlan() As Byte = Enumerable.Repeat(CByte(&H30), (40 * 4)).ToArray

            ' For every plan, except the last one, add a 0x00 at the end
            If plan < schedule.RingPlan.Length - 1 Then
                ringPlan = ringPlan.Append(&H0).ToArray
            End If

            ' Now we loop through all the ring times in the plan.
            For ring = 0 To schedule.RingPlan(plan).RingTime.Length - 1

                ' Map the current ringtime to a variable to save on typing. 
                Dim currentRing As BellMinderRingTime = schedule.RingPlan(plan).RingTime(ring)

                ' Skip empty rings
                If currentRing.TimeHH = 0 And currentRing.TimeMM = 0 And currentRing.TimeSS = 0 And currentRing.Duration = 0 Then
                    Continue For
                End If

                ' Also map the current hour to a variable
                Dim hour As Integer = currentRing.TimeHH

                ' If TimeAMPM is set, we need to change the hour to 24 hour, as that's what the controller expects
                If currentRing.TimeAMPM = 1 Then
                    hour = Date.ParseExact(hour, "hh", CultureInfo.InvariantCulture).ToString("HH")
                End If

                ' Now we make up our plan bytes.
                Dim ringBytes() As Byte = {
                    BitConverter.GetBytes(hour).First,
                    BitConverter.GetBytes(currentRing.TimeMM).First,
                    BitConverter.GetBytes(currentRing.TimeSS).First,
                    BitConverter.GetBytes(currentRing.Duration).First
                }

                ' Copy our four bytes, starting at index 0, to our ringPlan bytes, with an index of ring * 4.
                ' So if we're on plan 0, ring 1, we'll copy four bytes to byte 4-8. If we're on plan 0, ring 3, we'll copy four bytes to byte 16-20
                Array.Copy(ringBytes, 0, ringPlan, ring * 4, 4)

            Next
            ' Now we'll append our ringPlan (singular) to our ringPlans array.
            ringPlans = ringPlans.Concat(ringPlan).ToArray
        Next

        ' Create a an array of bytes, one for each day of BellMinder
        Dim dayBytes() As Byte = Enumerable.Repeat(CByte(&H0), _YearDays).ToArray

        ' Next we loop through all the days we have in the plan.
        For Each day In schedule.Day
            ' Parse the day we have
            Dim d As Date = Date.ParseExact(day.DayDate, "yyyy/MMM/dd", CultureInfo.InvariantCulture)

            ' When we tell the controller which plans to use, we send them using two half-bytes.
            ' If you send 21, then it'll use Alt plan 2, Main plan 1. 01 is no alt plan, but main plan 1
            Dim mainPlan As Byte = Nothing
            Dim altPlan As Byte = Nothing

            ' If the plan we're trying to use is "No Time Set" (which is a default plan
            ' created by the BellMinder software), we skip it, otherwise we prepare a byte 
            If schedule.RingPlan(day.Plan - 1).PlanName <> "No Time Set" Then
                mainPlan = BitConverter.GetBytes(day.Plan).First
            Else
                mainPlan = &H0
            End If

            ' Same as above, but with the AltPlan instead.
            ' I think it would work better if the schedule had a "IsEmpty" test or something,
            ' but that might be something to add later.
            If schedule.RingPlan(day.AltPlan - 1).PlanName <> "No Time Set" Then
                altPlan = BitConverter.GetBytes(day.AltPlan).First
            Else
                altPlan = &H0
            End If

            ' Now we construct a single byte from two half bytes by shifting the alt plan across by four bits,
            ' then OR-ing the main plan on the end. So if the plan is 1 and the alt-plan is 2, it'd look like this:
            ' Main = 00000001
            ' Main << 4 = 00010000
            ' Altplan = 00000010
            ' PlanByte = Altplan OR Main = 00100001
            ' PlanByte in hex = 21
            Dim planByte() As Byte = {(altPlan << 4) Or mainPlan}

            ' Work out what day of the "year" it is, keeping in mind that BellMinder goes on a 32 day month.
            ' We subtract one because we're working with a zero-indexed array
            Dim yearDay As Integer = CalculateDayOfYear(schedule.Year, d) - 1

            ' Now we copy our day byte into the correct spot in the dayBytes array
            Array.Copy(planByte, 0, dayBytes, yearDay, 1)
        Next

        ' Now we set up our final packet that will be sent. 
        Dim finalBytes() As Byte = {}

        ' Calculate how long our packet is going to be. It's everything, except the header bytes and the packet length bytes
        ' When we make the bytes, they'll be in the wrong order because endianness, so we need to reverse
        ' the bytes and trim off the first two (empty) bytes
        Dim packetLength As Byte() = BitConverter.GetBytes(dstStartBytes.Length + dstEndBytes.Length + ringCount.Length + ringPlans.Length + dayBytes.Length).Reverse.TakeLast(2).ToArray

        ' And we concatenate all the bytes together
        finalBytes = finalBytes.Concat({&H1B, &H42}).Concat(packetLength).Concat(dstStartBytes).Concat(dstEndBytes).Concat(ringCount).Concat(ringPlans).Concat(dayBytes).ToArray

        File.WriteAllBytes("file.bin", finalBytes)

        End

    End Sub

    ' BellMinder works on a 32 day month. No matter what month, it will ALWAYS have 32 days.
    ' This makes working out the "day of the year" difficult, so this function handles it for us. 
    Private Function CalculateDayOfYear(year As Integer, givenDate As Date) As Integer

        ' Get the start of the intended year as a date. 
        Dim startDate As Date = New Date(year, 1, 1)

        ' This is a lookup table of how many "extra" days there will be because of BellMinder's 32 day cycle.
        Dim bellMinderDays() As Integer = {1, 32 - Date.DaysInMonth(year, 2), 1, 2, 1, 2, 1, 1, 2, 1, 2, 1, 1, 32 - Date.DaysInMonth(year + 1, 2)}

        ' Next we calculate the days between the start day, and our given day. 
        Dim daysBetween As TimeSpan = givenDate.Subtract(startDate)

        ' Then we sum up how many extra days we've got. We subtract one because we're working with a zero-indexed array
        Dim extraDays As Integer = bellMinderDays.Take(givenDate.Month - 1).Sum()

        ' The extra days calculation breaks if you're in the next year, so we run an additional check.
        If givenDate.Year > year Then
            ' Get all the extra days up until that point and add them all together.
            ' Although we can't actually send more than 32 * 14 days to the
            ' controller (I think?), we'll make this future proof
            extraDays += bellMinderDays.Take(bellMinderDays.Length - givenDate.Month).Sum * (givenDate.Year - year) - 1
        Else
            ' Otherwise we need to add an extra day, because the difference between the 1/1/2023 and 1/1/2023 is 0 days
            extraDays += 1
        End If

        ' Now we return how many days have ACTUALLY elapsed, plus how many extra days have acculumated. 
        Return daysBetween.TotalDays + extraDays
    End Function

End Class
