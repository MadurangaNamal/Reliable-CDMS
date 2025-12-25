<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ReliableCDMS.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-12">
            <h2>Dashboard</h2>
            <p class="text-muted">Welcome to ReliableCDMS - Corporate Document Management System</p>
            <hr />
        </div>
    </div>

    <div class="row">
        <!-- Total Documents Card -->
        <div class="col-md-4 mb-4">
            <div class="card text-white bg-primary">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <h6 class="card-title">Total Documents</h6>
                            <h2 class="mb-0">
                                <asp:Label ID="lblTotalDocs" runat="server" Text="0"></asp:Label>
                            </h2>
                        </div>
                        <div>
                            <i class="fas fa-file-alt fa-3x"></i>
                        </div>
                    </div>
                </div>
                <div class="card-footer">
                    <a href="Documents.aspx" class="text-white text-decoration-none">
                        View all documents <i class="fas fa-arrow-right"></i>
                    </a>
                </div>
            </div>
        </div>

        <!-- Total Users Card (Admin only) -->
        <% if (Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin") { %>
        <div class="col-md-4 mb-4">
            <div class="card text-white bg-success">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <h6 class="card-title">Total Users</h6>
                            <h2 class="mb-0">
                                <asp:Label ID="lblTotalUsers" runat="server" Text="0"></asp:Label>
                            </h2>
                        </div>
                        <div>
                            <i class="fas fa-users fa-3x"></i>
                        </div>
                    </div>
                </div>
                <div class="card-footer">
                    <a href="Users.aspx" class="text-white text-decoration-none">
                        Manage users <i class="fas fa-arrow-right"></i>
                    </a>
                </div>
            </div>
        </div>
        <% } %>

        <!-- My Uploads Card -->
        <div class="col-md-4 mb-4">
            <div class="card text-white bg-info">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <h6 class="card-title">My Uploads</h6>
                            <h2 class="mb-0">
                                <asp:Label ID="lblMyUploads" runat="server" Text="0"></asp:Label>
                            </h2>
                        </div>
                        <div>
                            <i class="fas fa-upload fa-3x"></i>
                        </div>
                    </div>
                </div>
                <div class="card-footer">
                    <span class="text-white">Documents you've uploaded</span>
                </div>
            </div>
        </div>
    </div>

    <!-- Recent Documents Section -->
    <div class="row mt-4">
        <div class="col-md-12">
            <div class="card">
                <div class="card-header bg-primary text-white">
                    <h5 class="mb-0"><i class="fas fa-clock"></i> Recent Documents</h5>
                </div>
                <div class="card-body">
                    <asp:GridView ID="gvRecentDocuments" runat="server" 
                        CssClass="table table-striped table-hover" 
                        AutoGenerateColumns="False"
                        EmptyDataText="No documents found.">
                        <Columns>
                            <asp:BoundField DataField="FileName" HeaderText="File Name" />
                            <asp:BoundField DataField="Category" HeaderText="Category" />
                            <asp:BoundField DataField="UploadedByName" HeaderText="Uploaded By" />
                            <asp:BoundField DataField="UploadDate" HeaderText="Upload Date" DataFormatString="{0:MMM dd, yyyy}" />
                            <asp:BoundField DataField="CurrentVersion" HeaderText="Version" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- Quick Actions -->
    <div class="row mt-4">
        <div class="col-md-12">
            <div class="card">
                <div class="card-header bg-secondary text-white">
                    <h5 class="mb-0"><i class="fas fa-bolt"></i> Quick Actions</h5>
                </div>
                <div class="card-body">
                    <a href="Documents.aspx?focus=upload" class="btn btn-primary me-2">
                        <i class="fas fa-upload"></i> Upload Document
                    </a>
                    <a href="Documents.aspx?focus=search" class="btn btn-info me-2">
                        <i class="fas fa-search"></i> Search Documents
                    </a>

                    <% if (Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin") { %>
                    <a href="Users.aspx?focus=adduser" class="btn btn-success">
                        <i class="fas fa-user-plus"></i> Add User
                    </a>
                    <% } %>

                </div>
            </div>
        </div>
    </div>

</asp:Content>