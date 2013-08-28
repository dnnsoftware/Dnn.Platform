#Region "Copyright"

' 
' Copyright (c) [YEAR]
' by [OWNER]
' 

#End Region

#Region "Using Statements"

Imports System
Imports System.Web.Caching
Imports DotNetNuke.ComponentModel.DataAnnotations

#End Region

Namespace [OWNER].[MODULE]

    <TableName("[OWNER]_[MODULE]s")> _
    <PrimaryKey("[MODULE]ID")> _
    <Scope("ModuleID")> _
    <Cacheable("[MODULE]s", CacheItemPriority.[Default], 20)> _
    Public Class [MODULE]Info
        Public Property [MODULE]ID() As Integer
            Get
                Return m_[MODULE]ID
            End Get
            Set
                m_[MODULE]ID = Value
            End Set
        End Property
        Private m_[MODULE]ID As Integer
        Public Property ModuleID() As Integer
            Get
                Return m_ModuleID
            End Get
            Set
                m_ModuleID = Value
            End Set
        End Property
        Private m_ModuleID As Integer
        Public Property Title() As String
            Get
                Return m_Title
            End Get
            Set
                m_Title = Value
            End Set
        End Property
        Private m_Title As String
        Public Property Description() As String
            Get
                Return m_Description
            End Get
            Set
                m_Description = Value
            End Set
        End Property
        Private m_Description As String
        Public Property IsActive() As Boolean
            Get
                Return m_IsActive
            End Get
            Set
                m_IsActive = Value
            End Set
        End Property
        Private m_IsActive As Boolean
        Public Property CreatedOnDate() As DateTime
            Get
                Return m_CreatedOnDate
            End Get
            Set
                m_CreatedOnDate = Value
            End Set
        End Property
        Private m_CreatedOnDate As DateTime
        Public Property CreatedByUserID() As Integer
            Get
                Return m_CreatedByUserID
            End Get
            Set
                m_CreatedByUserID = Value
            End Set
        End Property
        Private m_CreatedByUserID As Integer
    End Class
End Namespace
