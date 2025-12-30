<%@ Page Title="Users" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="ReliableCDMS.Users" %>

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
            <h2>User Management</h2>
            <p class="text-muted">Manage system users (Admin Only)</p>
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

    <!-- Add User Section -->
    <div class="row mb-4" id="adduserSection">
        <div class="col-md-12">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0"><i class="fas fa-user-plus"></i>Add New User</h5>
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-3">
                            <div class="mb-3">
                                <label for="txtUsername" class="form-label">Username</label>
                                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" placeholder="Username"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvUsername" runat="server"
                                    ControlToValidate="txtUsername"
                                    ErrorMessage="Username is required"
                                    CssClass="text-danger"
                                    ValidationGroup="AddUser">
                                </asp:RequiredFieldValidator>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="mb-3">
                                <label for="txtPassword" class="form-label">Password</label>
                                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Password"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvPassword" runat="server"
                                    ControlToValidate="txtPassword"
                                    ErrorMessage="Password is required"
                                    CssClass="text-danger"
                                    ValidationGroup="AddUser">
                                </asp:RequiredFieldValidator>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="mb-3">
                                <label for="ddlRole" class="form-label">Role</label>
                                <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select">
                                    <asp:ListItem Value="Employee">Employee</asp:ListItem>
                                    <asp:ListItem Value="Manager">Manager</asp:ListItem>
                                    <asp:ListItem Value="Admin">Admin</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="mb-3">
                                <label for="ddlDepartment" class="form-label">Department</label>
                                <asp:DropDownList ID="ddlDepartment" runat="server" CssClass="form-select">
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
                            <asp:Button ID="btnAddUser" runat="server" Text="Add User"
                                CssClass="btn btn-primary w-100"
                                OnClick="btnAddUser_Click"
                                ValidationGroup="AddUser" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Users List -->
    <div class="row">
        <div class="col-md-12">
            <div class="card">
                <div class="card-header bg-secondary text-white">
                    <h5 class="mb-0"><i class="fas fa-users"></i>All Users</h5>
                </div>
                <div class="card-body">
                    <asp:GridView ID="gvUsers" runat="server"
                        CssClass="table table-striped table-hover"
                        AutoGenerateColumns="False"
                        DataKeyNames="UserId"
                        OnRowCommand="gvUsers_RowCommand"
                        AllowPaging="True"
                        PageSize="5"
                        OnPageIndexChanging="gvUsers_PageIndexChanging"
                        EmptyDataText="No users found.">

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
                            <asp:BoundField DataField="UserId" HeaderText="ID" />
                            <asp:BoundField DataField="Username" HeaderText="Username" />
                            <asp:BoundField DataField="Role" HeaderText="Role" />
                            <asp:BoundField DataField="Department" HeaderText="Department" />
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge bg-success" : "badge bg-danger" %>'>
                                        <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CreatedDate" HeaderText="Created Date" DataFormatString="{0:MMM dd, yyyy}" />
                            <asp:TemplateField HeaderText="Actions">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnActivate" runat="server"
                                        CssClass="btn btn-sm btn-success me-1"
                                        CommandName="ActivateUser"
                                        CommandArgument='<%# Eval("UserId") %>'
                                        CausesValidation="false"
                                        Visible='<%# !Convert.ToBoolean(Eval("IsActive")) %>'>
                                        <i class="fas fa-user-check"></i> Activate
                                    </asp:LinkButton>

                                    <asp:LinkButton ID="btnDeactivate" runat="server"
                                        CssClass="btn btn-sm btn-warning me-1"
                                        CommandName="DeleteUser"
                                        CommandArgument='<%# Eval("UserId") %>'
                                        OnClientClick="return confirm('Are you sure you want to deactivate this user?');"
                                        CausesValidation="false"
                                        Visible='<%# Convert.ToBoolean(Eval("IsActive")) %>'>
                                        <i class="fas fa-user-times"></i> Deactivate
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
        window.onload = function () {

            const params = new URLSearchParams(window.location.search);
            const focus = params.get('focus');

            let target = null;

            if (focus === 'adduser')
                target = document.getElementById('adduserSection');

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
