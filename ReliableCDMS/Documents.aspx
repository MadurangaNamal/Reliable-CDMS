<%@ Page Title="Documents" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Documents.aspx.cs" Inherits="ReliableCDMS.Documents" MaintainScrollPositionOnPostback="true" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .pagination-container table {
            margin: 20px auto;
        }

        .pagination-container td {
            padding: 2px 6px;
        }

        .pagination-container a {
            display: inline-block;
            padding: 8px 12px;
            margin: 0 2px;
            border: 1px solid #dee2e6;
            border-radius: 4px;
            color: #0d6efd;
            text-decoration: none;
            background-color: #fff;
            transition: all 0.3s;
        }

            .pagination-container a:hover {
                background-color: #0d6efd;
                color: #fff;
                border-color: #0d6efd;
            }

        .pagination-container span {
            display: inline-block;
            padding: 8px 12px;
            margin: 0 2px;
            border: 1px solid #0d6efd;
            border-radius: 4px;
            color: #fff;
            background-color: #0d6efd;
            font-weight: bold;
        }

        .modal-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.5);
            z-index: 1000;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .modal-dialog-custom {
            background: white;
            border-radius: 8px;
            padding: 0;
            max-width: 700px;
            width: 90%;
            max-height: 90vh;
            overflow-y: auto;
        }

        .highlight-section {
            border: 2px solid #0d6efd;
            box-shadow: 0 0 15px rgba(13,110,253,0.5);
            transition: all 0.6s ease;
        }
    </style>
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
    <div class="row mb-4" id="uploadSection">
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
                                <asp:FileUpload ID="fileUpload" runat="server" CssClass="form-control"
                                    onchange="validateFileSize(this)" />
                                <div id="fileSizeError" class="alert alert-danger mt-2" style="display: none;">
                                    File size exceeds 50MB limit. Please select a smaller file.
                                </div>
                                <asp:RequiredFieldValidator ID="rfvFile" runat="server"
                                    ControlToValidate="fileUpload"
                                    ErrorMessage="Please select a file"
                                    CssClass="text-danger"
                                    ValidationGroup="Upload">
                                </asp:RequiredFieldValidator>
                                <small class="form-text text-muted" id="selectedFileSize" style="display: flex"></small>
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
                        <div class="col-md-3">
                            <div class="mb-3">
                                <label for="txtComments" class="form-label">Comments (Optional)</label>
                                <asp:TextBox ID="txtComments" runat="server" CssClass="form-control"
                                    placeholder="Version comments..."></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-1">
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
    <div class="row mb-4" id="searchSection">
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
                        AllowPaging="True"
                        PageSize="5"
                        OnPageIndexChanging="gvDocuments_PageIndexChanging"
                        EmptyDataText="No documents found.">

                        <PagerSettings
                            Mode="NumericFirstLast"
                            FirstPageText="First"
                            LastPageText="Last"
                            PageButtonCount="5"
                            Position="Bottom" />

                        <PagerStyle
                            CssClass="pagination-container"
                            HorizontalAlign="Center" />

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

                    <div class="row mt-3">
                        <div class="col-md-6">
                            <asp:Label ID="lblPaginationInfo" runat="server" CssClass="text-muted"></asp:Label>
                        </div>
                        <div class="col-md-6 text-end">
                            <label class="text-muted">Items per page:</label>
                            <asp:DropDownList ID="ddlPageSize" runat="server" CssClass="form-select form-select-sm d-inline-block w-auto ms-2"
                                AutoPostBack="True" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged">
                                <asp:ListItem Value="5" Selected="True">5</asp:ListItem>
                                <asp:ListItem Value="10">10</asp:ListItem>
                                <asp:ListItem Value="25">25</asp:ListItem>
                                <asp:ListItem Value="50">50</asp:ListItem>
                                <asp:ListItem Value="100">100</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>

        function validateFileSize(input) {
            const maxSize = 52428800; // 50 MB in bytes
            const errorElement = document.getElementById('fileSizeError');
            const uploadButton = document.getElementById('<%= btnUpload.ClientID %>');
            const fileSizeElement = document.getElementById('selectedFileSize');

            if (input.files && input.files[0]) {
                const fileSize = input.files[0].size;
                const sizeMB = (fileSize / 1048576).toFixed(2);

                // Always show the file size
                fileSizeElement.textContent = `file size: ${sizeMB} MB`;

                if (fileSize > maxSize) {
                    errorElement.style.display = 'block';
                    uploadButton.disabled = true;
                    input.value = '';

                    return false;
                } else {
                    errorElement.style.display = 'none';
                    uploadButton.disabled = false;
                }
            } else {
                fileSizeElement.textContent = '';
                errorElement.style.display = 'none';
            }
            return true;
        }

        window.onload = function () {

            const params = new URLSearchParams(window.location.search);
            const focus = params.get('focus');

            let target = null;

            if (focus === 'upload') {
                target = document.getElementById('uploadSection');
            } else if (focus === 'search') {
                target = document.getElementById('searchSection');
            }

            if (target) {
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                target.classList.add('highlight-section');

                setTimeout(() => {
                    target.classList.remove('highlight-section');
                }, 3000);
            }
        };

    </script>
</asp:Content>
