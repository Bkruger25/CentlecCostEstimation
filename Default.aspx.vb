Imports System.Data.Sql
Imports System.Data.SqlClient
Imports Telerik.Web.UI
Imports iTextSharp
Imports iTextSharp.text
Imports iTextSharp.text.pdf
Imports iTextSharp.text.html
Imports System.IO

Partial Class _Default
    Inherits Page

    Dim CentConn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("Centlec08_v2").ConnectionString)
    Dim LocalConn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("CentlecLocalInfo").ConnectionString)
    Private Property cmd As SqlCommand
    Private Property cmd2 As SqlCommand
    Dim rdr As SqlDataReader

    'preload data for this project ID
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim prjid, query, query2 As String

        prjid = Request.QueryString("prjid")
        heading.Text = prjid

        If prjid = "" Then
            lblError.Visible = True
            lblError.Text = "No Project ID found!"
        Else
            'delete data for this project number
            'point data
            query = "delete from Design_Points where ProjectID = '" & prjid & "'"
            LocalConn.Open()
            cmd = New SqlCommand(query, LocalConn)
            cmd.ExecuteNonQuery()
            LocalConn.Close()
            'line data
            query = "delete from Design_Line where ProjectID = '" & prjid & "'"
            LocalConn.Open()
            cmd = New SqlCommand(query, LocalConn)
            cmd.ExecuteNonQuery()
            LocalConn.Close()

            'get points design for this project
            insertPointData("a685", "DistributionBoxType")
            insertPointData("a684", "EquipmentType")
            insertPointData("a689", "AssetType")
            insertPointData("a686", "MeterBoxType")
            insertPointData("a682", "PoleType")
            insertPointData("a688", "StayType")
            insertPointData("a687", "StreetlightType")

            'query = "select * from [FC_DESIGN_POINT] where ProjectID = '" & prjid & "'")
            'CentConn.Open()
            'cmd = New SqlCommand(query, CentConn)
            'rdr = cmd.ExecuteReader()
            'If rdr.HasRows Then
            '    Do While rdr.Read
            '        LocalConn.Open()
            '        query2 = "insert into Design_Points(ProjectID, AssetType, Count) Values('" & prjid & "', '" & rdr.Item("AssetType") & "', " & rdr.Item("No_Of_Lamps") & ")"
            '        cmd2 = New SqlCommand(query2, LocalConn)
            '        cmd2.ExecuteNonQuery()
            '        LocalConn.Close()
            '    Loop
            'End If
            'CentConn.Close()

            'get line design for this project
            insertLineData("a505", "AssetType")

            'query = "select * from [FC_DESIGN_LINE] where ProjectID = '" & prjid & "'"
            'CentConn.Open()
            'cmd = New SqlCommand(query, CentConn)
            'rdr = cmd.ExecuteReader()
            'If rdr.HasRows Then
            '    Do While rdr.Read
            '        Dim tempLen As Integer
            '        If IsDBNull(rdr.Item("Length")) Then
            '            tempLen = 0
            '        Else
            '            tempLen = rdr.Item("Length")
            '        End If
            '        LocalConn.Open()
            '        query2 = "insert into Design_Line(ProjectID, AssetType, Length) Values('" & prjid & "', '" & rdr.Item("AssetType") & "', " & tempLen & ")"
            '        cmd2 = New SqlCommand(query2, LocalConn)
            '        cmd2.ExecuteNonQuery()
            '        LocalConn.Close()
            '    Loop
            'End If
            'CentConn.Close()

            'update labour tarifs
            query = "Select * from [CW_GISCOE].[dbo].[TBL_LABOUR_TARRIF]"
            CentConn.Open()
            cmd = New SqlCommand(query, CentConn)
            rdr = cmd.ExecuteReader()
            If rdr.HasRows Then
                Dim tempQ As String
                'delete old data
                LocalConn.Open()
                tempQ = "delete from [Centlec_Cost_Estimation].[dbo].[TBL_LABOUR_TARRIF]"
                cmd2 = New SqlCommand(tempQ, LocalConn)
                cmd2.ExecuteNonQuery()
                'insert locally
                Do While rdr.Read
                    tempQ = "insert into [Centlec_Cost_Estimation].[dbo].[TBL_LABOUR_TARRIF] (DESCRIPTION, TARRIF_PH, CODE) VALUES('" & rdr.Item("DESCRIPTION") & "', " & rdr.Item("TARRIF_PH") & ", " & rdr.Item("CODE") & ")"
                    cmd2 = New SqlCommand(tempQ, LocalConn)
                    cmd2.ExecuteNonQuery()
                Loop
                LocalConn.Close()
            End If
            CentConn.Close()

        End If

    End Sub

    'Save Point Materials chosen for this project ID
    Protected Sub MaterialPointGrid_ItemCommand(sender As Object, e As GridCommandEventArgs) Handles MaterialPointGrid.ItemCommand
        If e.CommandName = "saveItems" Then
            Dim prjid, assetType, query As String
            Dim amount As Integer
            Dim item As GridDataItem = DirectCast(e.Item, GridDataItem)
            prjid = item("ProjectID").Text
            assetType = item("AssetType").Text
            amount = item("Amount").Text

            'insert into amount table materials
            LocalConn.Open()
            Dim combo As RadComboBox = DirectCast(item.FindControl("MaterialCB"), RadComboBox)
            Dim collection As IList(Of RadComboBoxItem) = combo.CheckedItems
            If (collection.Count <> 0) Then
                'delete old values from amount table
                'point data
                query = "delete from Amount_Points where ProjectID = '" & prjid & "' and AssetType = '" & assetType & "' and ELM = 'Material'"
                'LocalConn.Open()
                cmd = New SqlCommand(query, LocalConn)
                cmd.ExecuteNonQuery()
                'LocalConn.Close()

                CentConn.Open()
                For Each value As RadComboBoxItem In collection
                    'get unitcost for this item
                    Dim tempQ As String
                    Dim unitcost As Double
                    Dim cmdTemp As SqlCommand
                    tempQ = "select UNITCOST from [CW_GISCOE].[azteca].[MATERIALLEAF] where DESCRIPTION = '" & value.Text & "'"
                    cmdTemp = New SqlCommand(tempQ, CentConn)
                    rdr = cmdTemp.ExecuteReader()
                    If rdr.HasRows Then
                        Do While rdr.Read
                            If IsDBNull(rdr.Item("UNITCOST")) Then
                                unitcost = 0
                            Else
                                unitcost = rdr.Item("UNITCOST")
                            End If
                        Loop
                    Else
                        unitcost = 0
                    End If
                    'insert data into amounts table
                    query = "insert into Amount_Points(ProjectID, AssetType, Amount, AssetDescription, ELM, UnitCost) Values('" & prjid & "', '" & assetType & "', " & amount & ", '" & value.Text & "', 'Material', " & unitcost & ")"
                    cmd2 = New SqlCommand(query, LocalConn)
                    cmd2.ExecuteNonQuery()
                    rdr.Close()
                Next
                CentConn.Close()
                lblError.Text = "Items Saved."
                lblError.Visible = True
            End If
            LocalConn.Close()

            'insert into amount table equipment
            LocalConn.Open()
            Dim comboEQ As RadComboBox = DirectCast(item.FindControl("EquipmentCB"), RadComboBox)
            Dim collectionEQ As IList(Of RadComboBoxItem) = comboEQ.CheckedItems
            If (collectionEQ.Count <> 0) Then
                'delete old values from amount table
                'point data
                query = "delete from Amount_Points where ProjectID = '" & prjid & "' and AssetType = '" & assetType & "' and ELM = 'Equipment'"
                'LocalConn.Open()
                cmd = New SqlCommand(query, LocalConn)
                cmd.ExecuteNonQuery()
                'LocalConn.Close()

                CentConn.Open()
                For Each value As RadComboBoxItem In collectionEQ
                    'get unitcost for this item
                    Dim tempQ As String
                    Dim unitcost As Double
                    Dim cmdTemp As SqlCommand
                    tempQ = "select UNITCOST from [CW_GISCOE].[azteca].[EQUIPMENTLEAF] where DESCRIPTION = '" & value.Text & "'"
                    cmdTemp = New SqlCommand(tempQ, CentConn)
                    rdr = cmdTemp.ExecuteReader()
                    If rdr.HasRows Then
                        Do While rdr.Read
                            If IsDBNull(rdr.Item("UNITCOST")) Then
                                unitcost = 0
                            Else
                                unitcost = rdr.Item("UNITCOST")
                            End If
                        Loop
                    Else
                        unitcost = 0
                    End If
                    'insert data into amounts table
                    query = "insert into Amount_Points(ProjectID, AssetType, Amount, AssetDescription, ELM, UnitCost) Values('" & prjid & "', '" & assetType & "', " & amount & ", '" & value.Text & "', 'Equipment', " & unitcost & ")"
                    cmd2 = New SqlCommand(query, LocalConn)
                    cmd2.ExecuteNonQuery()
                    rdr.Close()
                Next
                CentConn.Close()
                lblError.Text = "Items Saved."
                lblError.Visible = True
            End If
            LocalConn.Close()

            'insert into amount table labour
            LocalConn.Open()
            Dim comboLB As RadComboBox = DirectCast(item.FindControl("LabourCB"), RadComboBox)
            Dim collectionLB As IList(Of RadComboBoxItem) = comboLB.CheckedItems
            If (collectionLB.Count <> 0) Then
                'delete old values from amount table
                'point data
                query = "delete from Amount_Points where ProjectID = '" & prjid & "' and AssetType = '" & assetType & "' and ELM = 'Labour'"
                'LocalConn.Open()
                cmd = New SqlCommand(query, LocalConn)
                cmd.ExecuteNonQuery()
                'LocalConn.Close()

                For Each value As RadComboBoxItem In collectionLB
                    'get unitcost for this item
                    Dim tempQ As String
                    Dim atrisan, apprentice, labourer As Double
                    Dim cmdTemp As SqlCommand
                    tempQ = "select * from [Centlec_Cost_Estimation].[dbo].[TBL_LABOUR_TARRIF]"
                    cmdTemp = New SqlCommand(tempQ, LocalConn)
                    rdr = cmdTemp.ExecuteReader()
                    If rdr.HasRows Then
                        Do While rdr.Read
                            Dim tempstr As String
                            tempstr = rdr.Item("DESCRIPTION")
                            tempstr = tempstr.Trim
                            Select Case tempstr
                                Case "Artisan"
                                    atrisan = rdr.Item("TARRIF_PH")
                                Case "Apprentice"
                                    apprentice = rdr.Item("TARRIF_PH")
                                Case "Labourer"
                                    labourer = rdr.Item("TARRIF_PH")
                            End Select
                        Loop
                    Else
                        atrisan = 0
                        apprentice = 0
                        labourer = 0
                    End If
                    rdr.Close()
                    'insert data into amounts table
                    query = "insert into Amount_Points(ProjectID, AssetType, Amount, AssetDescription, ELM, Artisan, Apprentice, Labourer) Values('" & prjid & "', '" & assetType & "', " & amount & ", '" & value.Text & "', 'Labour', " & atrisan & ", " & apprentice & ", " & labourer & ")"
                    cmd2 = New SqlCommand(query, LocalConn)
                    cmd2.ExecuteNonQuery()

                    'get times for each labour type
                    CentConn.Open()
                    query = "select * from [CW_GISCOE].[dbo].[TBL_LABOUR] where DESCRIPTION = '" & value.Text & "'"
                    Dim cmdTemp2 As SqlCommand
                    cmdTemp2 = New SqlCommand(query, CentConn)
                    rdr = cmdTemp2.ExecuteReader()
                    If rdr.HasRows Then
                        Do While rdr.Read
                            Dim insertQ As String
                            'Dim tempCMD As SqlCommand
                            insertQ = "update Amount_Points set Artisan_Time = @ARTISAN_TIME , Apprentice_Time = @APPRENTICE_TIME , Labourer_Time = @LABOURER_TIME where AssetDescription = @val"
                            'tempCMD = New SqlCommand(insertQ, LocalConn)
                            ' tempCMD.ExecuteNonQuery()
                            Using tempCMD As New SqlCommand
                                With tempCMD
                                    .Connection = LocalConn
                                    .CommandType = Data.CommandType.Text
                                    .CommandText = insertQ
                                    .Parameters.AddWithValue("@ARTISAN_TIME", rdr.Item("ARTISAN_TIME"))
                                    .Parameters.AddWithValue("@APPRENTICE_TIME", rdr.Item("APPRENTICE_TIME"))
                                    .Parameters.AddWithValue("@LABOURER_TIME", rdr.Item("LABOURER_TIME"))
                                    .Parameters.AddWithValue("@val", value.Text)
                                End With
                                Try
                                    tempCMD.ExecuteNonQuery()
                                Catch ex As Exception

                                End Try
                            End Using
                        Loop
                    End If
                    CentConn.Close()

                Next
                lblError.Text = "Items Saved."
                lblError.Visible = True
            End If
            LocalConn.Close()

            MaterialPointGrid.Rebind()

        End If
    End Sub

    'Save Line Materials chosen for this project ID
    Protected Sub MaterialGridLines_ItemCommand(sender As Object, e As GridCommandEventArgs) Handles MaterialGridLines.ItemCommand
        If e.CommandName = "saveItems" Then
            Dim prjid, assetType, query As String
            Dim lineLen As Integer
            Dim item As GridDataItem = DirectCast(e.Item, GridDataItem)
            prjid = item("ProjectID").Text
            assetType = item("AssetType").Text
            lineLen = item("Length").Text

            'insert into amount table materials
            LocalConn.Open()
            Dim combo As RadComboBox = DirectCast(item.FindControl("MaterialCB"), RadComboBox)
            Dim collection As IList(Of RadComboBoxItem) = combo.CheckedItems
            If (collection.Count <> 0) Then
                'delete old values from amount table
                'point data
                query = "delete from Amount_Line where ProjectID = '" & prjid & "' and AssetType = '" & assetType & "' and ELM = 'Material'"
                'LocalConn.Open()
                cmd = New SqlCommand(query, LocalConn)
                cmd.ExecuteNonQuery()
                'LocalConn.Close()

                For Each value As RadComboBoxItem In collection
                    'get unitcost for this item
                    Dim tempQ As String
                    Dim unitcost As Double
                    Dim cmdTemp As SqlCommand
                    CentConn.Open()
                    tempQ = "select UNITCOST from [CW_GISCOE].[azteca].[MATERIALLEAF] where DESCRIPTION = '" & value.Text & "'"
                    cmdTemp = New SqlCommand(tempQ, CentConn)
                    rdr = cmdTemp.ExecuteReader()
                    If rdr.HasRows Then
                        Do While rdr.Read
                            If IsDBNull(rdr.Item("UNITCOST")) Then
                                unitcost = 0
                            Else
                                unitcost = rdr.Item("UNITCOST")
                            End If
                        Loop
                    Else
                        unitcost = 0
                    End If
                    rdr.Close()
                    CentConn.Close()
                    query = "insert into Amount_Line(ProjectID, AssetType, Length, AssetDescription, ELM, UnitCost) Values('" & prjid & "', '" & assetType & "', " & lineLen & ", '" & value.Text & "', 'Material', " & unitcost & ")"
                    cmd2 = New SqlCommand(query, LocalConn)
                    cmd2.ExecuteNonQuery()
                Next
                lblError.Text = "Items Saved."
                lblError.Visible = True
            End If
            LocalConn.Close()

            'insert into amount table equipment
            LocalConn.Open()
            Dim comboEQ As RadComboBox = DirectCast(item.FindControl("EquipmentCB"), RadComboBox)
            Dim collectionEQ As IList(Of RadComboBoxItem) = comboEQ.CheckedItems
            If (collectionEQ.Count <> 0) Then
                'delete old values from amount table
                'point data
                query = "delete from Amount_Line where ProjectID = '" & prjid & "' and AssetType = '" & assetType & "' and ELM = 'Equipment'"
                'LocalConn.Open()
                cmd = New SqlCommand(query, LocalConn)
                cmd.ExecuteNonQuery()
                'LocalConn.Close()

                For Each value As RadComboBoxItem In collectionEQ
                    'get unitcost for this item
                    Dim tempQ As String
                    Dim unitcost As Double
                    Dim cmdTemp As SqlCommand
                    CentConn.Open()
                    tempQ = "select UNITCOST from [CW_GISCOE].[azteca].[EQUIPMENTLEAF] where DESCRIPTION = '" & value.Text & "'"
                    cmdTemp = New SqlCommand(tempQ, CentConn)
                    rdr = cmdTemp.ExecuteReader()
                    If rdr.HasRows Then
                        Do While rdr.Read
                            If IsDBNull(rdr.Item("UNITCOST")) Then
                                unitcost = 0
                            Else
                                unitcost = rdr.Item("UNITCOST")
                            End If
                        Loop
                    Else
                        unitcost = 0
                    End If
                    rdr.Close()
                    CentConn.Close()
                    query = "insert into Amount_Line(ProjectID, AssetType, Length, AssetDescription, ELM, UnitCost) Values('" & prjid & "', '" & assetType & "', " & lineLen & ", '" & value.Text & "', 'Equipment', " & unitcost & ")"
                    cmd2 = New SqlCommand(query, LocalConn)
                    cmd2.ExecuteNonQuery()
                Next
                lblError.Text = "Items Saved."
                lblError.Visible = True
            End If
            LocalConn.Close()

            'insert into amount table labour
            LocalConn.Open()
            Dim comboLB As RadComboBox = DirectCast(item.FindControl("LabourCB"), RadComboBox)
            Dim collectionLB As IList(Of RadComboBoxItem) = comboLB.CheckedItems
            If (collectionLB.Count <> 0) Then
                'delete old values from amount table
                'point data
                query = "delete from Amount_Line where ProjectID = '" & prjid & "' and AssetType = '" & assetType & "' and ELM = 'Labour'"
                'LocalConn.Open()
                cmd = New SqlCommand(query, LocalConn)
                cmd.ExecuteNonQuery()
                'LocalConn.Close()
                For Each value As RadComboBoxItem In collectionLB
                    'get unitcost for this item
                    Dim tempQ As String
                    Dim atrisan, apprentice, labourer As Double
                    Dim cmdTemp As SqlCommand
                    CentConn.Open()
                    tempQ = "select * from [Centlec_Cost_Estimation].[dbo].[TBL_LABOUR_TARRIF]"
                    cmdTemp = New SqlCommand(tempQ, LocalConn)
                    rdr = cmdTemp.ExecuteReader()
                    If rdr.HasRows Then
                        Do While rdr.Read
                            Dim tempstr As String
                            tempstr = rdr.Item("DESCRIPTION")
                            tempstr = tempstr.Trim
                            Select Case tempstr
                                Case "Artisan"
                                    atrisan = rdr.Item("TARRIF_PH")
                                Case "Apprentice"
                                    apprentice = rdr.Item("TARRIF_PH")
                                Case "Labourer"
                                    labourer = rdr.Item("TARRIF_PH")
                            End Select
                        Loop
                    Else
                        atrisan = 0
                        apprentice = 0
                        labourer = 0
                    End If
                    rdr.Close()
                    CentConn.Close()
                    query = "insert into Amount_Line(ProjectID, AssetType, Length, AssetDescription, ELM, Artisan, Apprentice, Labourer) Values('" & prjid & "', '" & assetType & "', " & lineLen & ", '" & value.Text & "', 'Labour', " & atrisan & ", " & apprentice & ", " & labourer & ")"
                    cmd2 = New SqlCommand(query, LocalConn)
                    cmd2.ExecuteNonQuery()

                    'get times for each labour type
                    CentConn.Open()
                    query = "select * from [CW_GISCOE].[dbo].[TBL_LABOUR] where DESCRIPTION = '" & value.Text & "'"
                    Dim cmdTemp2 As SqlCommand
                    cmdTemp2 = New SqlCommand(query, CentConn)
                    rdr = cmdTemp2.ExecuteReader()
                    If rdr.HasRows Then
                        Do While rdr.Read
                            Dim insertQ As String
                            'Dim tempCMD As SqlCommand
                            insertQ = "update Amount_Line set Artisan_Time = @ARTISAN_TIME , Apprentice_Time = @APPRENTICE_TIME , Labourer_Time = @LABOURER_TIME where AssetDescription = @val"
                            'tempCMD = New SqlCommand(insertQ, LocalConn)
                            ' tempCMD.ExecuteNonQuery()
                            Using tempCMD As New SqlCommand
                                With tempCMD
                                    .Connection = LocalConn
                                    .CommandType = Data.CommandType.Text
                                    .CommandText = insertQ
                                    .Parameters.AddWithValue("@ARTISAN_TIME", rdr.Item("ARTISAN_TIME"))
                                    .Parameters.AddWithValue("@APPRENTICE_TIME", rdr.Item("APPRENTICE_TIME"))
                                    .Parameters.AddWithValue("@LABOURER_TIME", rdr.Item("LABOURER_TIME"))
                                    .Parameters.AddWithValue("@val", value.Text)
                                End With
                                Try
                                    tempCMD.ExecuteNonQuery()
                                Catch ex As Exception

                                End Try
                            End Using
                        Loop
                    End If
                    CentConn.Close()
                Next
                lblError.Text = "Items Saved."
                lblError.Visible = True
            End If
            LocalConn.Close()

        End If
    End Sub

    'Save Equioment chosen for this project ID
    'Protected Sub EquipmentPointGrid_ItemCommand(sender As Object, e As GridCommandEventArgs) Handles EquipmentPointGrid.ItemCommand
    '    If e.CommandName = "saveItems" Then
    '        Dim prjid, assetType, query As String
    '        Dim amount As Integer
    '        Dim item As GridDataItem = DirectCast(e.Item, GridDataItem)
    '        prjid = item("ProjectID").Text
    '        assetType = item("AssetType").Text
    '        amount = item("Amount").Text

    '        'delete old values from amount table
    '        'point data
    '        query = "delete from Amount_Points where ProjectID = '" & prjid & "' and ELM = 'Equipment' and AssetType = '" & assetType & "'"
    '        LocalConn.Open()
    '        cmd = New SqlCommand(query, LocalConn)
    '        cmd.ExecuteNonQuery()
    '        LocalConn.Close()

    '        'insert into amount table
    '        LocalConn.Open()
    '        Dim combo As RadComboBox = DirectCast(item.FindControl("EquipmentCB"), RadComboBox)
    '        Dim collection As IList(Of RadComboBoxItem) = combo.CheckedItems
    '        If (collection.Count <> 0) Then
    '            For Each value As RadComboBoxItem In collection
    '                query = "insert into Amount_Points(ProjectID, AssetType, Amount, AssetDescription, ELM) Values('" & prjid & "', '" & assetType & "', " & amount & ", '" & value.Text & "', 'Equipment')"
    '                cmd2 = New SqlCommand(query, LocalConn)
    '                cmd2.ExecuteNonQuery()
    '            Next
    '            lblEquipError.Text = "Items Saved."
    '            lblEquipError.Visible = True
    '        End If
    '        LocalConn.Close()

    '    End If
    'End Sub

    'Save Line Equipment chosen for this project ID
    'Protected Sub EquipmentGridLines_ItemCommand(sender As Object, e As GridCommandEventArgs) Handles EquipmentGridLines.ItemCommand
    '    If e.CommandName = "saveItems" Then
    '        Dim prjid, assetType, query As String
    '        Dim lineLen As Integer
    '        Dim item As GridDataItem = DirectCast(e.Item, GridDataItem)
    '        prjid = item("ProjectID").Text
    '        assetType = item("AssetType").Text
    '        lineLen = item("Length").Text

    '        'delete old values from amount table
    '        'point data
    '        query = "delete from Amount_Line where ProjectID = '" & prjid & "' and ELM = 'Equipment' and AssetType = '" & assetType & "'"
    '        LocalConn.Open()
    '        cmd = New SqlCommand(query, LocalConn)
    '        cmd.ExecuteNonQuery()
    '        LocalConn.Close()

    '        'insert into amount table
    '        LocalConn.Open()
    '        Dim combo As RadComboBox = DirectCast(item.FindControl("EquipmentCB"), RadComboBox)
    '        Dim collection As IList(Of RadComboBoxItem) = combo.CheckedItems
    '        If (collection.Count <> 0) Then
    '            For Each value As RadComboBoxItem In collection
    '                query = "insert into Amount_Line(ProjectID, AssetType, Length, AssetDescription, ELM) Values('" & prjid & "', '" & assetType & "', " & lineLen & ", '" & value.Text & "', 'Equipment')"
    '                cmd2 = New SqlCommand(query, LocalConn)
    '                cmd2.ExecuteNonQuery()
    '            Next
    '            lblError.Text = "Items Saved."
    '            lblError.Visible = True
    '        End If
    '        LocalConn.Close()

    '    End If
    'End Sub


    'Save Quantities chosen for the materials
    Protected Sub btnSaveQty_Click(sender As Object, e As EventArgs) Handles btnSaveQty.Click
        Dim desc, query, prjid, assetType As String
        Dim qty As Integer

        prjid = Request.QueryString("prjid")
        'Dim nestedTableView As GridTableView = CType(MaterialPointGrid.MasterTableView.Items(0), GridDataItem).ChildItem.NestedTableViews(0)
        For Each row As GridDataItem In gridItems.Items
            desc = row("AssetDescription").Text
            assetType = row("AssetType").Text
            Dim txtbox As RadTextBox = DirectCast(row.FindControl("itemQty"), RadTextBox)
            If txtbox.Text <> "" Then
                qty = txtbox.Text

                query = "Update Amount_Points SET Quantity = " & qty & "WHERE ProjectID = '" & prjid & "' and AssetDescription = '" & desc & "' and AssetType = '" & assetType & "'"
                LocalConn.Open()
                cmd = New SqlCommand(query, LocalConn)
                cmd.ExecuteNonQuery()
                LocalConn.Close()
                lblError.Text = "Quantities Saved."
                lblError.Visible = True

            End If
            gridItems.Rebind()
        Next

    End Sub

    'Save Length chosen for the materials
    Protected Sub btnSaveLength_Click(sender As Object, e As EventArgs) Handles btnSaveLength.Click
        Dim desc, query, prjid, assetType As String
        Dim lineLen As Integer

        prjid = Request.QueryString("prjid")
        For Each row As GridDataItem In gridLineItems.Items
            desc = row("AssetDescription").Text
            assetType = row("AssetType").Text
            Dim txtbox As RadTextBox = DirectCast(row.FindControl("itemLength"), RadTextBox)
            If txtbox.Text <> "" Then
                lineLen = txtbox.Text

                query = "Update Amount_Line SET Length = " & lineLen & "WHERE ProjectID = '" & prjid & "' and AssetDescription = '" & desc & "' and AssetType = '" & assetType & "'"
                LocalConn.Open()
                cmd = New SqlCommand(query, LocalConn)
                cmd.ExecuteNonQuery()
                LocalConn.Close()
                lblError.Text = "Length Saved."
                lblError.Visible = True

            End If
            gridLineItems.Rebind()
        Next

    End Sub

    'show/hide grid Material point
    Protected Sub btnShowItems_Click(sender As Object, e As EventArgs) Handles btnShowItems.Click
        If btnShowItems.Text = "Show Items" Then
            gridItems.Visible = True
            lblShowItems.Visible = True
            btnSaveQty.Visible = True
            btnShowItems.Text = "Hide Items"
            btnRefreshGridItems.Visible = True
            gridItems.Rebind()
        Else
            gridItems.Visible = False
            lblShowItems.Visible = False
            btnSaveQty.Visible = False
            btnShowItems.Text = "Show Items"
            btnRefreshGridItems.Visible = False
        End If

    End Sub

    'show/hide grid Material Line
    Protected Sub btnShowLengthItems_Click(sender As Object, e As EventArgs) Handles btnShowLengthItems.Click
        If btnShowLengthItems.Text = "Show Items" Then
            gridLineItems.Visible = True
            lblShowLineItems.Visible = True
            btnSaveLength.Visible = True
            btnShowLengthItems.Text = "Hide Items"
            btnRefreshgridLineItems.Visible = True
            gridLineItems.Rebind()
        Else
            gridLineItems.Visible = False
            lblShowLineItems.Visible = False
            btnSaveLength.Visible = False
            btnShowLengthItems.Text = "Show Items"
            btnRefreshgridLineItems.Visible = False
        End If

    End Sub

    'show/hide grid Equipment point
    'Protected Sub btnShowEquipmentItems_Click(sender As Object, e As EventArgs) Handles btnShowEquipmentItems.Click
    '    If btnShowEquipmentItems.Text = "Show Items" Then
    '        gridEquipmentItems.Visible = True
    '        lblEquipmentShowItems.Visible = True
    '        btnSaveEquipmentQty.Visible = True
    '        btnShowEquipmentItems.Text = "Hide Items"
    '        btnRefreshEquipmentGridItems.Visible = True
    '        gridEquipmentItems.Rebind()
    '    Else
    '        gridEquipmentItems.Visible = False
    '        lblEquipmentShowItems.Visible = False
    '        btnSaveEquipmentQty.Visible = False
    '        btnShowEquipmentItems.Text = "Show Items"
    '        btnRefreshEquipmentGridItems.Visible = False
    '    End If

    'End Sub

    'show/hide grid Equipment Line
    'Protected Sub btnShowEquipmentLineItems_Click(sender As Object, e As EventArgs) Handles btnShowEquipmentLineItems.Click
    '    If btnShowEquipmentLineItems.Text = "Show Items" Then
    '        gridEquipmentLineItems.Visible = True
    '        lblShowEquipmentLineItems.Visible = True
    '        btnSaveEquipmentLineLength.Visible = True
    '        btnShowEquipmentLineItems.Text = "Hide Items"
    '        btnRefreshEquipmentLineItems.Visible = True
    '        gridEquipmentLineItems.Rebind()
    '    Else
    '        gridEquipmentLineItems.Visible = False
    '        lblShowEquipmentLineItems.Visible = False
    '        btnSaveEquipmentLineLength.Visible = False
    '        btnShowEquipmentLineItems.Text = "Show Items"
    '        btnRefreshEquipmentLineItems.Visible = False
    '    End If

    'End Sub

    'rebind grid items ELM point and line
    Protected Sub btnRefreshGridItems_Click(sender As Object, e As EventArgs) Handles btnRefreshGridItems.Click
        gridItems.Rebind()
    End Sub

    Protected Sub btnRefreshgridLineItems_Click(sender As Object, e As EventArgs) Handles btnRefreshgridLineItems.Click
        gridLineItems.Rebind()
    End Sub

    Protected Sub btnGenerateInvoice_Click(sender As Object, e As EventArgs) Handles btnGenerateInvoice.Click

        Dim prjid As String = Request.QueryString("prjid")

        'create doc + image path
        Dim imagepath As String = Server.MapPath("images")
        Dim doc As New Document()
        'open doc
        Dim wri As PdfWriter = PdfWriter.GetInstance(doc, New FileStream(Server.MapPath("Invoices\" + prjid + ".pdf"), FileMode.Create))
        doc.Open()
        'add logo to doc
        Dim jpg As Image = Image.GetInstance(imagepath & "/Centlec_Logo.png")
        jpg.Alignment = Element.ALIGN_MIDDLE
        jpg.ScaleToFit(250.0F, 250.0F)
        jpg.BorderWidthBottom = 1
        doc.Add(jpg)

        doc.Add(New Paragraph(vbLf))
        'add company info
        Dim p1 As New Paragraph("Centlec")
        Dim p8 As New Paragraph("Phone: +123 123 123", FontFactory.GetFont("Verdana", 8))
        Dim p9 As New Paragraph("info@123.co.za", FontFactory.GetFont("Verdana", 8))

        'set alignment and add to document
        p1.Alignment = Element.ALIGN_LEFT
        p8.Alignment = Element.ALIGN_LEFT
        p9.Alignment = Element.ALIGN_LEFT
        doc.Add(p1)
        doc.Add(p8)
        doc.Add(p9)

        'add empty line
        doc.Add(New Paragraph(vbLf))

        'subheader
        Dim pMaterials = New Phrase("MATERIALS", FontFactory.GetFont("Times New Roman", 10, Font.BOLD))
        doc.Add(pMaterials)

        'add empty line
        doc.Add(New Paragraph(vbLf))

        'add table
        doc.Add(New Paragraph(vbLf))
        Dim bfTimes As BaseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, False)
        Dim table2 As New PdfPTable(4)
        table2.WidthPercentage = 100
        'invoice headers
        Dim cell4 As New PdfPCell(New Phrase("ASSET DESCRIPTION", New Font(bfTimes, 15, Font.BOLD)))
        cell4.HorizontalAlignment = 1
        table2.AddCell(cell4)

        Dim cell5 As New PdfPCell(New Phrase("QTY/LENGTH", New Font(bfTimes, 15, Font.BOLD)))
        cell5.HorizontalAlignment = 1
        table2.AddCell(cell5)

        Dim cell6 As New PdfPCell(New Phrase("UNIT COST", New Font(bfTimes, 15, Font.BOLD)))
        cell6.HorizontalAlignment = 1
        table2.AddCell(cell6)

        Dim cell7 As New PdfPCell(New Phrase("AMOUNT", New Font(bfTimes, 15, Font.BOLD)))
        cell7.HorizontalAlignment = 1
        table2.AddCell(cell7)

        'get items chosen point table Materials
        Dim totalAmountMaterials, qty, assetAmount As Double
        totalAmountMaterials = 0
        LocalConn.Open()
        Dim cmdTemp As SqlCommand
        Dim reader As SqlDataReader
        'points table
        Dim query As String = "select * from [Centlec_Cost_Estimation].[dbo].[Amount_Points] where ProjectID = '" & prjid & "' and ELM = 'Material'"
        cmdTemp = New SqlCommand(query, LocalConn)
        reader = cmdTemp.ExecuteReader()
        If reader.HasRows Then
            Do While reader.Read
                table2.AddCell(" " & reader.Item("AssetDescription"))
                table2.AddCell(" " & reader.Item("Quantity"))
                table2.AddCell(" " & reader.Item("UnitCost"))
                Dim finalAmount As Double
                Dim tempUC As Double
                Dim amnt As Double
                If IsDBNull(reader.Item("UnitCost")) Then
                    tempUC = 0
                Else
                    tempUC = reader.Item("UnitCost")
                End If
                If IsDBNull(reader.Item("Quantity")) Then
                    qty = 0
                Else
                    qty = reader.Item("Quantity")
                End If
                assetAmount = reader.Item("Amount")
                finalAmount = tempUC * qty * assetAmount
                totalAmountMaterials += finalAmount
                
                table2.AddCell(" " & finalAmount)
            Loop
        End If
        reader.Close()
        'lines table
        query = "select * from [Centlec_Cost_Estimation].[dbo].[Amount_Line] where ProjectID = '" & prjid & "' and ELM = 'Material'"
        cmdTemp = New SqlCommand(query, LocalConn)
        reader = cmdTemp.ExecuteReader()
        If reader.HasRows Then
            Do While reader.Read
                table2.AddCell(" " & reader.Item("AssetDescription"))
                table2.AddCell(" " & reader.Item("Length"))
                table2.AddCell(" " & reader.Item("UnitCost"))
                Dim finalAmount As Double
                Dim tempUC As Double
                Dim amnt As Double
                If IsDBNull(reader.Item("UnitCost")) Then
                    tempUC = 0
                Else
                    tempUC = reader.Item("UnitCost")
                End If
                If IsDBNull(reader.Item("Length")) Then
                    qty = 0
                Else
                    qty = reader.Item("Length")
                End If
                finalAmount = tempUC * qty
                totalAmountMaterials += finalAmount

                table2.AddCell(" " & finalAmount)
            Loop
        End If
        reader.Close()
        LocalConn.Close()

        'for subtotal Materials
        'table2.AddCell(" ")
        Dim blank1 As New PdfPCell(New Phrase(" "))
        blank1.Border = 0
        table2.AddCell(blank1)
        Dim blank2 As New PdfPCell(New Phrase(" "))
        blank2.Border = 0
        table2.AddCell(blank2)
        Dim cell8 As New PdfPCell(New Phrase("TOTAL: ", New Font(bfTimes, 12, Font.BOLD)))
        cell8.HorizontalAlignment = 2
        '0=Left, 1=Centre, 2=Right
        cell8.Border = 0
        table2.AddCell(cell8)
        'add total amount
        Dim cell9 As New PdfPCell(New Phrase("R " & Format(Val(totalAmountMaterials), "0.00"), New Font(bfTimes, 12, Font.BOLD)))
        cell9.HorizontalAlignment = 2
        '0=Left, 1=Centre, 2=Right
        'cell8.BackgroundColor = New BaseColor(220, 220, 220)
        table2.AddCell(cell9)

        'add table to doc
        doc.Add(table2)
        doc.NewPage()

        'subheader
        Dim pEquipment = New Phrase("EQUIPMENT", FontFactory.GetFont("Times New Roman", 10, Font.BOLD))
        doc.Add(pEquipment)

        'add table for Equipment
        doc.Add(New Paragraph(vbLf))
        Dim table3 As New PdfPTable(4)
        table3.WidthPercentage = 100
        'invoice headers
        Dim cell43 As New PdfPCell(New Phrase("ASSET DESCRIPTION", New Font(bfTimes, 15, Font.BOLD)))
        cell43.HorizontalAlignment = 1
        table3.AddCell(cell43)

        Dim cell53 As New PdfPCell(New Phrase("QTY/LENGTH", New Font(bfTimes, 15, Font.BOLD)))
        cell53.HorizontalAlignment = 1
        table3.AddCell(cell53)

        Dim cell63 As New PdfPCell(New Phrase("UNIT COST", New Font(bfTimes, 15, Font.BOLD)))
        cell63.HorizontalAlignment = 1
        table3.AddCell(cell63)

        Dim cell73 As New PdfPCell(New Phrase("AMOUNT", New Font(bfTimes, 15, Font.BOLD)))
        cell73.HorizontalAlignment = 1
        table3.AddCell(cell73)

        'get items chosen point table Equiptment
        Dim totalAmountEquimpment As Double
        totalAmountEquimpment = 0
        LocalConn.Open()
        'Dim cmdTemp As SqlCommand
        'Dim reader As SqlDataReader
        query = "select * from [Centlec_Cost_Estimation].[dbo].[Amount_Points] where ProjectID = '" & prjid & "' and ELM = 'Equipment'"
        cmdTemp = New SqlCommand(query, LocalConn)
        reader = cmdTemp.ExecuteReader()
        If reader.HasRows Then
            Do While reader.Read
                table3.AddCell(" " & reader.Item("AssetDescription"))
                table3.AddCell(" " & reader.Item("Quantity"))
                table3.AddCell(" " & reader.Item("UnitCost"))
                Dim finalAmount As Double
                Dim tempUC As Double
                If IsDBNull(reader.Item("UnitCost")) Then
                    tempUC = 0
                Else
                    tempUC = reader.Item("UnitCost")
                End If
                If IsDBNull(reader.Item("Quantity")) Then
                    qty = 0
                Else
                    qty = reader.Item("Quantity")
                End If
                assetAmount = reader.Item("Amount")
                finalAmount = tempUC * qty * assetAmount
                totalAmountEquimpment += finalAmount

                table3.AddCell(" " & finalAmount)
            Loop
        End If
        reader.Close()
        'lines table
        query = "select * from [Centlec_Cost_Estimation].[dbo].[Amount_Line] where ProjectID = '" & prjid & "' and ELM = 'Equipment'"
        cmdTemp = New SqlCommand(query, LocalConn)
        reader = cmdTemp.ExecuteReader()
        If reader.HasRows Then
            Do While reader.Read
                table3.AddCell(" " & reader.Item("AssetDescription"))
                table3.AddCell(" " & reader.Item("Length"))
                table3.AddCell(" " & reader.Item("UnitCost"))
                Dim finalAmount As Double
                Dim tempUC As Double
                Dim amnt As Double
                If IsDBNull(reader.Item("UnitCost")) Then
                    tempUC = 0
                Else
                    tempUC = reader.Item("UnitCost")
                End If
                If IsDBNull(reader.Item("Length")) Then
                    qty = 0
                Else
                    qty = reader.Item("Length")
                End If
                finalAmount = tempUC * qty
                totalAmountEquimpment += finalAmount

                table3.AddCell(" " & finalAmount)
            Loop
        End If
        reader.Close()
        LocalConn.Close()

        'for subtotal equiopment
        table3.AddCell(blank1)
        table3.AddCell(blank2)
        Dim cell83 As New PdfPCell(New Phrase("TOTAL: ", New Font(bfTimes, 12, Font.BOLD)))
        cell83.HorizontalAlignment = 2
        '0=Left, 1=Centre, 2=Right
        cell83.Border = 0
        table3.AddCell(cell83)
        'add total amount
        Dim cell93 As New PdfPCell(New Phrase("R " & Format(Val(totalAmountEquimpment), "0.00"), New Font(bfTimes, 12, Font.BOLD)))
        cell93.HorizontalAlignment = 2
        '0=Left, 1=Centre, 2=Right
        'cell8.BackgroundColor = New BaseColor(220, 220, 220)
        table3.AddCell(cell93)

        'add table to doc
        doc.Add(table3)
        doc.NewPage()

        'subheader
        Dim pLabour = New Phrase("LABOUR", FontFactory.GetFont("Times New Roman", 10, Font.BOLD))
        doc.Add(pLabour)

        'add table for Labour
        doc.Add(New Paragraph(vbLf))
        Dim table4 As New PdfPTable(4)
        table4.WidthPercentage = 100
        'invoice headers
        Dim cell44 As New PdfPCell(New Phrase("ASSET DESCRIPTION", New Font(bfTimes, 15, Font.BOLD)))
        cell44.HorizontalAlignment = 1
        table4.AddCell(cell44)

        Dim cell54 As New PdfPCell(New Phrase("QTY/LENGTH", New Font(bfTimes, 15, Font.BOLD)))
        cell54.HorizontalAlignment = 1
        table4.AddCell(cell54)

        Dim cell64 As New PdfPCell(New Phrase("LABOUR RATES", New Font(bfTimes, 15, Font.BOLD)))
        cell64.HorizontalAlignment = 1
        table4.AddCell(cell64)

        Dim cell74 As New PdfPCell(New Phrase("AMOUNT", New Font(bfTimes, 15, Font.BOLD)))
        cell74.HorizontalAlignment = 1
        table4.AddCell(cell74)

        ''get items chosen point table Labour
        Dim totalAmountLabour As Double
        Dim artTime, appTime, labTime As Double
        Dim labTimeAmount As String
        totalAmountLabour = 0
        labTimeAmount = ""
        LocalConn.Open()
        'Dim cmdTemp As SqlCommand
        'Dim reader As SqlDataReader
        query = "select * from [Centlec_Cost_Estimation].[dbo].[Amount_Points] where ProjectID = '" & prjid & "' and ELM = 'Labour'"
        cmdTemp = New SqlCommand(query, LocalConn)
        reader = cmdTemp.ExecuteReader()
        If reader.HasRows Then
            Do While reader.Read
                table4.AddCell(" " & reader.Item("AssetDescription"))
                table4.AddCell(" " & reader.Item("Quantity"))
                'table4.AddCell(" " & reader.Item("UnitCost"))
                Dim finalAmount As Long

                If IsDBNull(reader.Item("Artisan_Time")) Then
                    artTime = 1
                Else
                    artTime = reader.Item("Artisan_Time") * reader.Item("Artisan")
                    labTimeAmount += reader.Item("Artisan_Time") & " x " & reader.Item("Artisan") & vbNewLine
                End If
                If IsDBNull(reader.Item("Apprentice_Time")) Then
                    appTime = 1
                Else
                    appTime = reader.Item("Apprentice_Time") * reader.Item("Apprentice")
                    labTimeAmount += reader.Item("Apprentice_Time") & " x " & reader.Item("Apprentice") & vbNewLine
                End If
                If IsDBNull(reader.Item("Labourer_Time")) Then
                    labTime = 1
                Else
                    labTime = reader.Item("Labourer_Time") * reader.Item("Labourer")
                    labTimeAmount += reader.Item("Labourer_Time") & " x " & reader.Item("Labourer") & vbNewLine
                End If
                If IsDBNull(reader.Item("Quantity")) Then
                    qty = 0
                Else
                    qty = reader.Item("Quantity")
                End If
                table4.AddCell(" " & labTimeAmount)
                assetAmount = reader.Item("Amount")
                If artTime <> 1 Then
                    finalAmount += (artTime * qty * assetAmount)
                End If
                If appTime <> 1 Then
                    finalAmount += (artTime * qty * assetAmount)
                End If
                If labTime <> 1 Then
                    finalAmount += (labTime * qty * assetAmount)
                End If
                'finalAmount = artTime + appTime + labTime * qty * assetAmount
                totalAmountLabour += finalAmount

                table4.AddCell(" " & finalAmount)
            Loop
        End If
        reader.Close()

        'get items chosen Lines table Labour
        labTimeAmount = ""
        'Dim cmdTemp As SqlCommand
        'Dim reader As SqlDataReader
        query = "select * from [Centlec_Cost_Estimation].[dbo].[Amount_Line] where ProjectID = '" & prjid & "' and ELM = 'Labour'"
        cmdTemp = New SqlCommand(query, LocalConn)
        reader = cmdTemp.ExecuteReader()
        If reader.HasRows Then
            Do While reader.Read
                table4.AddCell(" " & reader.Item("AssetDescription"))
                table4.AddCell(" " & reader.Item("Length"))
                'table4.AddCell(" " & reader.Item("UnitCost"))
                Dim finalAmount As Double = 0

                If IsDBNull(reader.Item("Artisan_Time")) Then
                    artTime = 1
                Else
                    artTime = reader.Item("Artisan_Time") * reader.Item("Artisan")
                    labTimeAmount += reader.Item("Artisan_Time") & " x " & reader.Item("Artisan") & vbNewLine
                End If
                If IsDBNull(reader.Item("Apprentice_Time")) Then
                    appTime = 1
                Else
                    appTime = reader.Item("Apprentice_Time") * reader.Item("Apprentice")
                    labTimeAmount += reader.Item("Apprentice_Time") & " x " & reader.Item("Apprentice") & vbNewLine
                End If
                If IsDBNull(reader.Item("Labourer_Time")) Then
                    labTime = 1
                Else
                    labTime = reader.Item("Labourer_Time") * reader.Item("Labourer")
                    labTimeAmount += reader.Item("Labourer_Time") & " x " & reader.Item("Labourer") & vbNewLine
                End If
                If IsDBNull(reader.Item("Length")) Then
                    qty = 0
                Else
                    qty = reader.Item("Length")
                End If
                table4.AddCell(" " & labTimeAmount)
                If artTime <> 1 Then
                    finalAmount += (artTime * qty)
                End If
                If appTime <> 1 Then
                    finalAmount += (artTime * qty)
                End If
                If labTime <> 1 Then
                    finalAmount += (labTime * qty)
                End If
                'finalAmount = (artTime * qty * assetAmount) + (appTime * qty * assetAmount) + (labTime * qty * assetAmount)
                totalAmountLabour += finalAmount

                table4.AddCell(" " & finalAmount)
            Loop
        End If
        reader.Close()
        LocalConn.Close()

        'for subtotal Labour
        'table2.AddCell(" ")
        table4.AddCell(blank1)
        table4.AddCell(blank2)
        Dim cell84 As New PdfPCell(New Phrase("TOTAL: ", New Font(bfTimes, 12, Font.BOLD)))
        cell84.HorizontalAlignment = 2
        '0=Left, 1=Centre, 2=Right
        cell84.Border = 0
        table4.AddCell(cell84)
        'add total amount
        Dim cell94 As New PdfPCell(New Phrase("R " & Format(Val(totalAmountLabour), "0.00"), New Font(bfTimes, 12, Font.BOLD)))
        cell94.HorizontalAlignment = 2
        '0=Left, 1=Centre, 2=Right
        'cell8.BackgroundColor = New BaseColor(220, 220, 220)
        table4.AddCell(cell94)

        'add table to doc
        doc.Add(table4)
        'add empty line
        doc.Add(New Paragraph(vbLf))

        Dim p12 = New Phrase("THANK YOU FOR YOUR BUSINESS!", FontFactory.GetFont("Times New Roman", 10, Font.BOLD))
        doc.Add(p12)

        doc.Close()

        Dim path As String = Server.MapPath("~/Invoices/" & prjid & ".pdf") 'get file object as FileInfo
        Dim file As System.IO.FileInfo = New System.IO.FileInfo(path) '-- if the file exists on the server
        If file.Exists Then 'set appropriate headers
            Response.Clear()
            Response.AddHeader("Content-Disposition", "attachment; filename=" & file.Name)
            Response.AddHeader("Content-Length", file.Length.ToString())
            Response.ContentType = "Application/pdf"
            Response.TransmitFile(file.FullName)
            Response.End() 'if file does not exist
        Else
            Response.Write("This file does not exist.")
        End If 'nothing in the URL as HTTP GET
    End Sub

    Protected Sub insertPointData(tbl As String, col As String)
        Dim prjid, query, query2 As String

        prjid = Request.QueryString("prjid")
        heading.Text = prjid

        'get points design for this project
        query= " select * from " & tbl & " where ProjectID = '" & prjid & "'"
        CentConn.Open()
        cmd = New SqlCommand(Query, CentConn)
        rdr = cmd.ExecuteReader()
        If rdr.HasRows Then
            Do While rdr.Read
                'count asset amount
                Dim tempCMD As SqlCommand
                Dim tempQuery As String
                Dim tempRdr As SqlDataReader
                Dim tempCount As Integer
                Dim tempCentConn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("Centlec08_v2").ConnectionString)
                tempCentConn.Open()
                tempCount = 0
                tempQuery = " select * from " & tbl & " where ProjectID = '" & prjid & "' and " & col & "= '" & rdr.Item(col) & "'"
                tempCMD = New SqlCommand(tempQuery, tempCentConn)
                tempRdr = tempCMD.ExecuteReader
                Do While tempRdr.Read
                    tempCount = tempCount + 1
                Loop
                tempCentConn.Close()

                LocalConn.Open()
                query2 = "insert into Design_Points(ProjectID, AssetType, Count) Values(@prjid, @col, @noLamps)"
                Using cmd2 As New SqlCommand
                    With cmd2
                        .Connection = LocalConn
                        .CommandType = Data.CommandType.Text
                        .CommandText = query2
                        .Parameters.AddWithValue("@prjid", prjid)
                        .Parameters.AddWithValue("@col", rdr.Item(col))
                        If tempCount = 0 Then
                            .Parameters.AddWithValue("@noLamps", 0)
                        Else
                            .Parameters.AddWithValue("@noLamps", tempCount)
                        End If

                    End With
                    Try
                        cmd2.ExecuteNonQuery()
                    Catch ex As Exception

                    End Try
                End Using
                LocalConn.Close()
            Loop
        End If
        CentConn.Close()
    End Sub

    Protected Sub insertLineData(tbl As String, col As String)
        Dim prjid, query, query2 As String

        prjid = Request.QueryString("prjid")
        heading.Text = prjid

        query = "select * from " & tbl & " where ProjectID = '" & prjid & "'"
        CentConn.Open()
        cmd = New SqlCommand(Query, CentConn)
        rdr = cmd.ExecuteReader()
        If rdr.HasRows Then
            Do While rdr.Read
                Dim tempLen As Integer
                If IsDBNull(rdr.Item("Length")) Then
                    tempLen = 0
                Else
                    tempLen = rdr.Item("Length")
                End If
                LocalConn.Open()
                query2 = "insert into Design_Line(ProjectID, AssetType, Length) Values(@prjid, @col,@tempLen)"
                Using cmd2 As New SqlCommand
                    With cmd2
                        .Connection = LocalConn
                        .CommandType = Data.CommandType.Text
                        .CommandText = query2
                        .Parameters.AddWithValue("@prjid", prjid)
                        .Parameters.AddWithValue("@col", rdr.Item(col))
                        .Parameters.AddWithValue("@tempLen", tempLen)
                    End With
                    Try
                        cmd2.ExecuteNonQuery()
                    Catch ex As Exception

                    End Try
                End Using
                LocalConn.Close()
            Loop
        End If
        CentConn.Close()
    End Sub

End Class