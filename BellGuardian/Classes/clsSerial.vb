﻿Imports System.ComponentModel
Imports System.IO
Imports System.IO.Ports
Imports System.Reflection
Public Class clsSerial

    ' Internal reference to our serial port object
    Private _SerialPort As SerialPort = Nothing

    <Description("Is the COM port open?"), DisplayName("COM Port Open"), Category("Properties")>
    Public Property IsOpen As Boolean = False

    <Description("The baud rate of the serial port"), DisplayName("Baud Rate"), Category("Properties")>
    Public Property BaudRate As Integer = 4800

    Public Property PortName As String = "COM3"

    Public Sub New(Name As String, Optional Rate As Integer = Nothing)
        BaudRate = Rate
        PortName = Name
    End Sub

    <Description("Opens the COM port using the specified PortName and BaudRate"), DisplayName("Open Port"), Category("Methods")>
    Public Sub Open()
        _SerialPort = New SerialPort(PortName, BaudRate, Parity.None, 8, 1)
    End Sub

    <Description("Close the COM port if it's open. Does nothing if not"), DisplayName("Close Port"), Category("Methods")>
    Public Sub Close()
        If _SerialPort IsNot Nothing Then
            _SerialPort.Close()
        End If
    End Sub

    ' Generated by ChatGPT and tweaked by David Gray, 
    ' this sends hex data to the given serial port on the specified baud rate.
    <Description("Send data to the COM port"), DisplayName("Send Data"), Category("Methods")>
    Public Function Send(dataToSend As Byte(), Optional read As Boolean = False) As String
        Try
            ' Create a new SerialPort instance
            Using _SerialPort

                ' If the COM port isn't open. 
                If Not IsOpen Then
                    Throw New Exception("Serial port not open!")
                End If

                ' Send the data to the serial port
                _SerialPort.Write(dataToSend, 0, dataToSend.Length)

                ' If we want to read the data it returns, do so here, otherwise return Nothing
                If read Then
                    Return _SerialPort.ReadLine()
                Else
                    Return Nothing
                End If

            End Using
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

End Class
