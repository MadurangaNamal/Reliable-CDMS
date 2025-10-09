<%@ Page Title="Documents" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Documents.aspx.cs" Inherits="ReliableCDMS.Documents" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-12">
            <h2>Document Management</h2>
            <p class="text-muted">Upload, search, and manage documents</p>
            <hr />
        </div>
    </div>

    <!-- Success/Error Messages -->
    <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
        <asp:Label ID="lblSuccess" runat="server"></asp:Label>
    </asp:Panel>
    <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
        <asp:Label ID="lblError" runat="server"></asp:Label>
    </asp:Panel>

    <!-- Upload Document Section -->
    <div class="row mb-4">
        <div class="col-md-12">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0"><i class="fas fa-upload"></i> Upload Document</h5>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="mb-3">
                                <label for="fileUpload" class="form-label">Select File</label>
                                <asp:FileUpload ID="fileUpload" runat="server" CssClass="form-control" />
                                <asp:RequiredFieldValidator ID="rfvFile" runat="server" 
                                    ControlToValidate="fileUpload" 
                                    ErrorMessage="Please select a file" 
                                    CssClass="text-danger"
                                    ValidationGroup="Upload">
                                </asp:RequiredFieldValidator>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="mb-3">
                                <label for="ddlCategory" class="form-label">Category</label>
                                <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select">
                                    <asp:ListItem Value="General">General</asp:ListItem>
                                    <asp:ListItem Value="HR">HR</asp:ListItem>
                                    <asp:ListItem Value="Finance">Finance</asp:ListItem>
                                    <asp:ListItem Value="IT">IT</asp:ListItem>
                                    <asp:ListItem Value="Sales">Sales</asp:ListItem>
                                    <asp:ListItem Value="Marketing">Marketing</asp:ListItem>
                                    <asp:ListItem Value="Legal">Legal</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">&nbsp;</label>
                            <asp:Button ID="btnUpload" runat="server" Text="Upload" 
                                CssClass="btn btn-primary w-100" 
                                OnClick="btnUpload_Click"
                                ValidationGroup="Upload" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Search Section -->
    <div class="row mb-4">
        <div class="col-md-12">
            <div class="card">
                <div class="card-header bg-info text-white">
                    <h5 class="mb-0"><i class="fas fa-search"></i> Search Documents</h5>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-10">
                            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" 
                                placeholder="Search by filename or category..."></asp:TextBox>
                        </div>
                        <div class="col-md-2">
                            <asp:Button ID="btnSearch" runat="server" Text="Search" 
                                CssClass="btn btn-info w-100" 
                                OnClick="btnSearch_Click"
                                CausesValidation="false" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Documents List -->
    <div class="row">
        <div class="col-md-12">
            <div class="card">
                <div class="card-header bg-secondary text-white">
                    <h5 class="mb-0"><i class="fas fa-folder-open"></i> All Documents</h5>
                </div>
                <div class="card-body">
                    <asp:GridView ID="gvDocuments" runat="server" 
                        CssClass="table table-striped table-hover" 
                        AutoGenerateColumns="False"
                        DataKeyNames="DocumentId"
                        OnRowCommand="gvDocuments_RowCommand"
                        EmptyDataText="No documents found.">
                        <Columns>
                            <asp:BoundField DataField="DocumentId" HeaderText="ID" />
                            <asp:BoundField DataField="FileName" HeaderText="File Name" />
                            <asp:BoundField DataField="Category" HeaderText="Category" />
                            <asp:BoundField DataField="UploadedByName" HeaderText="Uploaded By" />
                            <asp:BoundField DataField="UploadDate" HeaderText="Upload Date" DataFormatString="{0:MMM dd, yyyy HH:mm}" />
                            <asp:BoundField DataField="CurrentVersion" HeaderText="Version" />
                            <asp:TemplateField HeaderText="Size">
                                <ItemTemplate>
                                    <%# FormatFileSize(Eval("FileSize")) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnDownload" runat="server" 
                                        CssClass="btn btn-sm btn-success me-1"
                                        CommandName="Download" 
                                        CommandArgument='<%# Eval("DocumentId") %>'
                                        CausesValidation="false">
                                        <i class="fas fa-download"></i> Download
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnDelete" runat="server" 
                                        CssClass="btn btn-sm btn-danger"
                                        CommandName="DeleteDoc" 
                                        CommandArgument='<%# Eval("DocumentId") %>'
                                        OnClientClick="return confirm('Are you sure you want to delete this document?');"
                                        CausesValidation="false">
                                        <i class="fas fa-trash"></i> Delete
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>