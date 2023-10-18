Imports System.Xml.Serialization

Public Class frmMain
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim bm As New clsBellMinder("COM3")
        Dim schedule As clsBellMinderSchedule = bm.LoadSchedule("C:\Users\graydav1.LAVALLA\OneDrive - Lavalla Catholic College\Documents\GitHub\bellminder-generator\Lavalla 2023 Bell Schedule.sch")
        bm.SendSchedule(schedule)
    End Sub
End Class
