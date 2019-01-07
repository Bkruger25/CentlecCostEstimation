<%@ Page Title="Home Page" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1 style="text-align:center">Bill of Materials for Project: </h1>
        <h1 style="text-align:center"><asp:Literal ID="heading" runat="server"></asp:Literal></h1>
    </div>

    <telerik:RadSkinManager ID="QsfSkinManager" runat="server" Skin="Silk" />

     <telerik:RadButton ID="btnGenerateInvoice" runat="server" Text="Generate Bill of Materials"></telerik:RadButton>

    <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server">
    </telerik:RadAjaxLoadingPanel>
    <telerik:RadAjaxPanel ID="RadAjaxPanel1" runat="server" LoadingPanelID="RadAjaxLoadingPanel1">

        <div class="row">
            <div class="col-md-12">
                <h2>Point Assets</h2>
                <telerik:RadGrid ID="MaterialPointGrid" runat="server" CellSpacing="-1" DataSourceID="PointsData" GridLines="Both" GroupPanelPosition="Top">
                    <MasterTableView AutoGenerateColumns="False" DataSourceID="PointsData" DataKeyNames="AssetType" >
                        
                        <DetailTables>
                            <telerik:GridTableView  runat="server" DataSourceID="showItems" GridLines="Both" DataKeyNames="AssetType"  AutoGenerateColumns="false">
                                <Columns>
                                    <telerik:GridBoundColumn DataField="AssetType" FilterControlAltText="Filter AssetType column" HeaderText="AssetType" SortExpression="AssetType" UniqueName="AssetType">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="AssetDescription" FilterControlAltText="Filter AssetDescription column" HeaderText="AssetDescription" SortExpression="AssetDescription" UniqueName="AssetDescription">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="Quantity" FilterControlAltText="Filter Quantity column" HeaderText="Quantity" SortExpression="Quantity" UniqueName="Quantity">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="ELM" FilterControlAltText="Filter ELM column" HeaderText="E/L/M" SortExpression="ELM" UniqueName="ELM">
                                    </telerik:GridBoundColumn>                                    
                                </Columns>
                            </telerik:GridTableView>
                        </DetailTables>
                        <ParentTableRelation>
                            <telerik:GridRelationFields DetailKeyField="ProjectID" MasterKeyField="ProjectID" />
                         </ParentTableRelation>

                        <Columns>
                            <telerik:GridBoundColumn DataField="ProjectID" FilterControlAltText="Filter ProjectID column" HeaderText="ProjectID" SortExpression="ProjectID" UniqueName="ProjectID">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="AssetType" FilterControlAltText="Filter AssetType column" HeaderText="AssetType" SortExpression="AssetType" UniqueName="AssetType">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Amount" DataType="System.Int32" FilterControlAltText="Filter Amount column" HeaderText="Count" SortExpression="Amount" UniqueName="Amount" ReadOnly="True">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="Materials">
                                <ItemTemplate>
                                    <telerik:RadComboBox RenderMode="Lightweight" ID="MaterialCB" runat="server" DataSourceID="materials" DataTextField="DESCRIPTION" DataValueField="DESCRIPTION"
                                        CheckBoxes="true" EnableCheckAllItemsCheckBox="true" Width="250" ></telerik:RadComboBox>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Equipment">
                                <ItemTemplate>
                                    <telerik:RadComboBox RenderMode="Lightweight" ID="EquipmentCB" runat="server" DataSourceID="equipment" DataTextField="DESCRIPTION" DataValueField="DESCRIPTION"
                                        CheckBoxes="true" EnableCheckAllItemsCheckBox="true" Width="250" ></telerik:RadComboBox>                                                                
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Labour">
                                <ItemTemplate>
                                    <telerik:RadComboBox RenderMode="Lightweight" ID="LabourCB" runat="server" DataSourceID="labour" DataTextField="DESCRIPTION" DataValueField="DESCRIPTION"
                                        CheckBoxes="true" EnableCheckAllItemsCheckBox="true" Width="250" ></telerik:RadComboBox>                                      
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridButtonColumn HeaderText="Save Items" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" UniqueName="saveItems" CommandName="saveItems" ButtonType="PushButton" Text="Save Items"></telerik:GridButtonColumn>
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
                <asp:SqlDataSource ID="materials"  runat="server" ConnectionString="<%$ ConnectionStrings:CW_GISCOE %>" SelectCommand="SELECT * FROM [CW_GISCOE].[azteca].[MATERIALLEAF]"></asp:SqlDataSource>
                <asp:SqlDataSource ID="equipment"  runat="server" ConnectionString="<%$ ConnectionStrings:CW_GISCOE %>" SelectCommand="SELECT * FROM [CW_GISCOE].[azteca].[EQUIPMENTLEAF]"></asp:SqlDataSource>
                <asp:SqlDataSource ID="labour"  runat="server" ConnectionString="<%$ ConnectionStrings:CW_GISCOE %>" SelectCommand="SELECT * FROM [CW_GISCOE].[dbo].[TBL_LABOUR]"></asp:SqlDataSource> 
                <asp:SqlDataSource ID="PointsData" runat="server" ConnectionString="<%$ ConnectionStrings:CentlecLocalInfo %>" SelectCommand="SELECT DISTINCT [ProjectID], [AssetType], SUM([Count]) AS Amount FROM [Design_Points]
                        where ProjectID = @prjID
                        group by [ProjectID], [AssetType]">
                    <SelectParameters>
                        <asp:QueryStringParameter DefaultValue="&quot;&quot;" Name="prjID" QueryStringField="prjid" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <telerik:RadButton ID="btnShowItems" runat="server" Text="Show Items"></telerik:RadButton>

                <br /><br />
                <asp:Label ID="lblShowItems" runat="server" Text="Please enter a Quantity:"  ForeColor="Red" Visible="false"></asp:Label>
                <telerik:RadGrid ID="gridItems" runat="server" CellSpacing="-1" DataSourceID="showItems" GridLines="Both" GroupPanelPosition="Top" Visible="false">
                    <MasterTableView AutoGenerateColumns="False" DataSourceID="showItems">
                        <Columns>
                            <telerik:GridBoundColumn DataField="AssetType" FilterControlAltText="Filter AssetType column" HeaderText="AssetType" SortExpression="AssetType" UniqueName="AssetType">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="AssetDescription" FilterControlAltText="Filter AssetDescription column" HeaderText="AssetDescription" SortExpression="AssetDescription" UniqueName="AssetDescription">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Quantity" FilterControlAltText="Filter Quantity column" HeaderText="Quantity" SortExpression="Quantity" UniqueName="Quantity">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="Enter Quantity">
                                <ItemTemplate>
                                    <telerik:RadTextBox runat="server"  ID="itemQty"></telerik:RadTextBox>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
                <asp:SqlDataSource ID="showItems" runat="server" ConnectionString="<%$ ConnectionStrings:CentlecLocalInfo %>" SelectCommand="SELECT DISTINCT ProjectID, [AssetType], [AssetDescription], Quantity, ELM FROM [Amount_Points]  where ProjectID = @prjID">
                        <SelectParameters>
                        <asp:QueryStringParameter DefaultValue="&quot;&quot;" Name="prjID" QueryStringField="prjid" />
                    </SelectParameters>
                </asp:SqlDataSource>
                 <asp:SqlDataSource ID="pointItemsDetailedView" runat="server" ConnectionString="<%$ ConnectionStrings:CentlecLocalInfo %>" SelectCommand="SELECT * FROM [Amount_Points] WHERE ([AssetType] = @AssetType)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="gridItems" Name="AssetType" PropertyName="SelectedValue" Type="String" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <telerik:RadButton ID="btnSaveQty" runat="server" Text="Save Quantities" Visible="false"></telerik:RadButton>
                <telerik:RadButton ID="btnRefreshGridItems" runat="server" Text="Refresh Items" Visible="false"></telerik:RadButton>
            </div>
        </div>

            <div class="row">
            <div class="col-md-12">
                <h2>Line Assets</h2>
                    <telerik:RadGrid ID="MaterialGridLines" runat="server" CellSpacing="-1" DataSourceID="LineData" GridLines="Both" GroupPanelPosition="Top" AllowMultiRowEdit="True" AllowMultiRowSelection="True">
                    <MasterTableView AutoGenerateColumns="False" DataSourceID="LineData" DataKeyNames="AssetType">

                        <DetailTables>
                            <telerik:GridTableView  runat="server" DataSourceID="lineItemsDetailedView" GridLines="Both" DataKeyNames="AssetType"  AutoGenerateColumns="false">
                                <ParentTableRelation>                                    
                                    <telerik:GridRelationFields DetailKeyField="AssetType" MasterKeyField="AssetType" />
                                </ParentTableRelation>
                                <Columns>
                                    <telerik:GridBoundColumn DataField="AssetType" FilterControlAltText="Filter AssetType column" HeaderText="AssetType" SortExpression="AssetType" UniqueName="AssetType">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="AssetDescription" FilterControlAltText="Filter AssetDescription column" HeaderText="AssetDescription" SortExpression="AssetDescription" UniqueName="AssetDescription">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="Length" FilterControlAltText="Filter Quantity column" HeaderText="Length" SortExpression="Length" UniqueName="Length">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="ELM" FilterControlAltText="Filter ELM column" HeaderText="E/L/M" SortExpression="ELM" UniqueName="ELM">
                                    </telerik:GridBoundColumn>                              
                                </Columns>
                            </telerik:GridTableView>
                        </DetailTables>

                        <Columns>
                            <telerik:GridBoundColumn DataField="ProjectID" FilterControlAltText="Filter ProjectID column" HeaderText="ProjectID" SortExpression="ProjectID" UniqueName="ProjectID">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="AssetType" FilterControlAltText="Filter AssetType column" HeaderText="AssetType" SortExpression="AssetType" UniqueName="AssetType">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Length" DataType="System.Int32" FilterControlAltText="Filter Length column" HeaderText="Length" SortExpression="Length" UniqueName="Length" ReadOnly="True">
                            </telerik:GridBoundColumn>
                                <telerik:GridTemplateColumn HeaderText="Materials">
                                <ItemTemplate>
                                    <telerik:RadComboBox RenderMode="Lightweight" ID="MaterialCB" runat="server" DataSourceID="materials" DataTextField="DESCRIPTION" DataValueField="DESCRIPTION"
                                        CheckBoxes="true" EnableCheckAllItemsCheckBox="true" Width="250" ></telerik:RadComboBox>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Equipment">
                                <ItemTemplate>
                                    <telerik:RadComboBox RenderMode="Lightweight" ID="EquipmentCB" runat="server" DataSourceID="equipment" DataTextField="DESCRIPTION" DataValueField="DESCRIPTION"
                                        CheckBoxes="true" EnableCheckAllItemsCheckBox="true" Width="250" ></telerik:RadComboBox>                                
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Labour">
                                <ItemTemplate>
                                    <telerik:RadComboBox RenderMode="Lightweight" ID="LabourCB" runat="server" DataSourceID="labour" DataTextField="DESCRIPTION" DataValueField="DESCRIPTION"
                                        CheckBoxes="true" EnableCheckAllItemsCheckBox="true" Width="250" ></telerik:RadComboBox>                           
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridButtonColumn HeaderText="Save Items" ItemStyle-HorizontalAlign="Center" ItemStyle-VerticalAlign="Middle" UniqueName="saveItems" CommandName="saveItems" ButtonType="PushButton" Text="Save Items">
                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Middle"></ItemStyle>
                            </telerik:GridButtonColumn>
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>            
                <asp:SqlDataSource ID="LineData" runat="server" ConnectionString="<%$ ConnectionStrings:CentlecLocalInfo %>" SelectCommand="SELECT DISTINCT [ProjectID], [AssetType], SUM([Length]) AS Length FROM [Design_Line]
                        where ProjectID = @prjID
                        group by [ProjectID], [AssetType]">
                    <SelectParameters>
                        <asp:QueryStringParameter DefaultValue="&quot;&quot;" Name="prjID" QueryStringField="prjid" />
                    </SelectParameters>
                </asp:SqlDataSource>
                    <telerik:RadButton ID="btnShowLengthItems" runat="server" Text="Show Items"></telerik:RadButton>

                    <br /><br />
                <asp:Label ID="lblShowLineItems" runat="server" Text="Please enter a Length:"  ForeColor="Red" Visible="false"></asp:Label>
                <telerik:RadGrid ID="gridLineItems" runat="server" CellSpacing="-1" DataSourceID="showLineItems" GridLines="Both" GroupPanelPosition="Top" Visible="false">
                    <MasterTableView AutoGenerateColumns="False" DataSourceID="showLineItems">
                        <Columns>
                            <telerik:GridBoundColumn DataField="AssetType" FilterControlAltText="Filter AssetType column" HeaderText="AssetType" SortExpression="AssetType" UniqueName="AssetType">
                            </telerik:GridBoundColumn>
                                <telerik:GridBoundColumn DataField="AssetDescription" FilterControlAltText="Filter AssetDescription column" HeaderText="AssetDescription" SortExpression="AssetDescription" UniqueName="AssetDescription">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Length" FilterControlAltText="Filter Length column" HeaderText="Length" SortExpression="Length" UniqueName="Length">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="Enter Length">
                                <ItemTemplate>
                                    <telerik:RadTextBox runat="server"  ID="itemLength"></telerik:RadTextBox>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
                <asp:SqlDataSource ID="showLineItems" runat="server" ConnectionString="<%$ ConnectionStrings:CentlecLocalInfo %>" SelectCommand="SELECT DISTINCT ProjectID,  [AssetType], AssetDescription,  SUM([Length]) AS Length FROM [Amount_Line]
                        where ProjectID = @prjID
                        group by [ProjectID], [AssetType], AssetDescription">
                            <SelectParameters>
                            <asp:QueryStringParameter DefaultValue="&quot;&quot;" Name="prjID" QueryStringField="prjid" />
                        </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="lineItemsDetailedView" runat="server" ConnectionString="<%$ ConnectionStrings:CentlecLocalInfo %>" SelectCommand="SELECT * FROM [Amount_Line] WHERE ([AssetType] = @AssetType)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="gridLineItems" Name="AssetType" PropertyName="SelectedValue" Type="String" />
                        </SelectParameters>
                </asp:SqlDataSource>
                <telerik:RadButton ID="btnSaveLength" runat="server" Text="Save Lengths" Visible="false"></telerik:RadButton>
                <telerik:RadButton ID="btnRefreshgridLineItems" runat="server" Text="Refresh Items" Visible="false"></telerik:RadButton>
            </div>
        </div>

        <asp:Label ID="lblError" runat="server" Text="" ForeColor="Red" Visible="false"></asp:Label>
</telerik:RadAjaxPanel>
    
</asp:Content>
