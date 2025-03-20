class PermissionsGrid {
    constructor(options) {
        this.options = {
            container: '.dnnPermissionsGrid',
            updateUrl: '',
            addRoleUrl: '',
            addUserUrl: '',
            ...options
        };

        this.init();
    }

    init() {
        this.container = $(this.options.container);
        
        // Initialize tokeninput for user search
        this.initUserSearch();
        
        // Bind event handlers
        this.bindEvents();
        
        // Initialize tri-state checkboxes
        this.initTriStateCheckboxes();
    }

    bindEvents() {
        // Role group dropdown change
        this.container.find('.roleGroupsDropDown').on('change', (e) => {
            this.onRoleGroupChange(e);
        });

        // Add role button click
        this.container.find('.addRoleBtn').on('click', (e) => {
            e.preventDefault();
            this.addRole();
        });

        // Add user button click
        this.container.find('.addUserBtn').on('click', (e) => {
            e.preventDefault();
            this.addUser();
        });

        // Delete role click
        this.container.find('.deleteRole').on('click', (e) => {
            e.preventDefault();
            this.deleteRole($(e.currentTarget));
        });

        // Delete user click
        this.container.find('.deleteUser').on('click', (e) => {
            e.preventDefault();
            this.deleteUser($(e.currentTarget));
        });

        // Permission change
        this.container.find('.permissionTriState').on('change', (e) => {
            this.updatePermissions();
        });
    }

    initUserSearch() {
        const $userSearch = this.container.find('.userSearchInput');
        const serviceFramework = $.ServicesFramework();
        
        $userSearch.tokenInput('/DesktopModules/Admin/Security/API/Users/Search', {
            theme: 'facebook',
            resultsFormatter: (item) => {
                return `<li>${item.displayName} (${item.userName})</li>`;
            },
            tokenFormatter: (item) => {
                return `<li>${item.displayName}</li>`;
            },
            preventDuplicates: true,
            tokenLimit: 1,
            onAdd: (item) => {
                this.container.find('#hiddenUserIds').val(item.id);
            },
            onDelete: () => {
                this.container.find('#hiddenUserIds').val('');
            },
            hintText: 'Type to search users',
            noResultsText: 'No results',
            searchingText: 'Searching...',
            tokenValue: 'id',
            propertyToSearch: 'displayName',
            prePopulate: null,
            animateDropdown: false,
            processPrePopulate: false
        });
    }

    initTriStateCheckboxes() {
        this.container.find('.permissionTriState').each((i, el) => {
            const $triState = $(el);
            $triState.triState({
                state: $triState.data('state'),
                enabled: $triState.data('enabled')
            });
        });
    }

    onRoleGroupChange(e) {
        const groupId = $(e.target).val();
        const $rolesDropDown = this.container.find('.rolesDropDown');
        
        $.ajax({
            url: '/API/Security/GetRoles',
            data: { groupId: groupId },
            success: (roles) => {
                $rolesDropDown.empty();
                roles.forEach(role => {
                    $rolesDropDown.append(new Option(role.RoleName, role.RoleID));
                });
            }
        });
    }

    addRole() {
        const roleId = this.container.find('.rolesDropDown').val();
        
        $.ajax({
            url: this.options.addRoleUrl,
            type: 'POST',
            data: { roleId: roleId },
            success: (response) => {
                if (response.success) {
                    window.location.reload();
                } else {
                    alert(response.message);
                }
            }
        });
    }

    addUser() {
        const userId = this.container.find('#hiddenUserIds').val();
        if (!userId) {
            alert('Please select a user');
            return;
        }

        $.ajax({
            url: this.options.addUserUrl,
            type: 'POST',
            data: { userId: userId },
            success: (response) => {
                if (response.success) {
                    window.location.reload();
                } else {
                    alert(response.message);
                }
            }
        });
    }

    deleteRole(button) {
        if (!confirm('Are you sure you want to delete this role permission?')) {
            return;
        }

        const row = button.closest('tr');
        const roleId = row.data('role-id');
        this.deletePermissions('role', roleId, row);
    }

    deleteUser(button) {
        if (!confirm('Are you sure you want to delete this user permission?')) {
            return;
        }

        const row = button.closest('tr');
        const userId = row.data('user-id');
        this.deletePermissions('user', userId, row);
    }

    deletePermissions(type, id, row) {
        const permissions = [];
        row.find('.permissionTriState').each((i, el) => {
            const $triState = $(el);
            permissions.push({
                permissionId: $triState.data('permission-id'),
                isRolePermission: type === 'role',
                roleId: type === 'role' ? id : null,
                userId: type === 'user' ? id : null,
                permissionKey: 'Null'
            });
        });

        this.updatePermissionsOnServer(permissions, () => {
            row.remove();
        });
    }

    updatePermissions() {
        const permissions = [];
        
        // Collect role permissions
        this.container.find('.rolePermissions tr[data-role-id]').each((i, row) => {
            const $row = $(row);
            const roleId = $row.data('role-id');
            
            $row.find('.permissionTriState').each((j, checkbox) => {
                const $checkbox = $(checkbox);
                permissions.push({
                    permissionId: $checkbox.data('permission-id'),
                    isRolePermission: true,
                    roleId: roleId,
                    userId: null,
                    permissionKey: $checkbox.triState('state')
                });
            });
        });

        // Collect user permissions
        this.container.find('.userPermissions tr[data-user-id]').each((i, row) => {
            const $row = $(row);
            const userId = $row.data('user-id');
            
            $row.find('.permissionTriState').each((j, checkbox) => {
                const $checkbox = $(checkbox);
                permissions.push({
                    permissionId: $checkbox.data('permission-id'),
                    isRolePermission: false,
                    roleId: null,
                    userId: userId,
                    permissionKey: $checkbox.triState('state')
                });
            });
        });

        this.updatePermissionsOnServer(permissions);
    }

    updatePermissionsOnServer(permissions, callback) {
        $.ajax({
            url: this.options.updateUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ permissions: permissions }),
            success: (response) => {
                if (response.success) {
                    if (callback) {
                        callback();
                    }
                } else {
                    alert(response.message);
                }
            }
        });
    }
}

// jQuery plugin
(function($) {
    $.fn.permissionsGrid = function(options) {
        return this.each(function() {
            if (!$.data(this, 'permissionsGrid')) {
                $.data(this, 'permissionsGrid', new PermissionsGrid({
                    container: this,
                    ...options
                }));
            }
        });
    };
})(jQuery); 