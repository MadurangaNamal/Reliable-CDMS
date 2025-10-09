<%@ Page Title="Users" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Users.aspx.cs" Inherits="ReliableCDMS.Users" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
    <div class="row mb-4">
        <div class="col-md-12">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0"><i class="fas fa-user-plus"></i> Add New User</h5>
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
                                <label for="txtDepartment" class="form-label">Department</label>
                                <asp:TextBox ID="txtDepartment" runat="server" CssClass="form-control" placeholder="Department"></asp:TextBox>
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
                    <h5 class="mb-0"><i class="fas fa-users"></i> All Users</h5>
                </div>
                <div class="card-body">
                    <asp:GridView ID="gvUsers" runat="server" 
                        CssClass="table table-striped table-hover" 
                        AutoGenerateColumns="False"
                        DataKeyNames="UserId"
                        OnRowCommand="gvUsers_RowCommand"
                        EmptyDataText="No users found.">
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
                                    <asp:LinkButton ID="btnDeleteUser" runat="server" 
                                        CssClass="btn btn-sm btn-danger"
                                        CommandName="DeleteUser" 
                                        CommandArgument='<%# Eval("UserId") %>'
                                        OnClientClick="return confirm('Are you sure you want to deactivate this user?');"
                                        CausesValidation="false">
                                        <i class="fas fa-user-times"></i> Deactivate
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