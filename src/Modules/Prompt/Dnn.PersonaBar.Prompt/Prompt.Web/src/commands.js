exports.commands = function() {
  return [
    {name: 'cls', flags: []},
    {name: 'console', flags: []},
    {name: 'reload', flags: []},
    {name: 'get-module', flags: ['id']},
    {name: 'list-modules', flags: ['name', 'title', 'all']},
    {name: 'get-page', flags: ['id', 'name', 'parentid']},
    {name: 'list-pages', flags: ['parentid']},
    {name: 'set-page', flags: ['description', 'id', 'keywords', 'name', 'title', 'visible']},
    {name: 'get-portal', flags: ['id']},
    {name: 'list-roles', flags: []},
    {name: 'new-role', flags: ['autoassign', 'description', 'name', 'public']},
    {name: 'set-role', flags: ['description', 'id', 'name', 'public']},
    {name: 'get-task', flags: ['id']},
    {name: 'list-tasks', flags: ['enabled', 'name']},
    {name: 'set-task', flags: ['enabled', 'id']},
    {name: 'add-roles', flags: ['end', 'id', 'roles', 'start']},
    {name: 'delete-user', flags: ['id', 'notify']},
    {name: 'get-user', flags: ['email', 'id', 'username']},
    {name: 'list-users', flags: ['email', 'role', 'email']},
    {
      name: 'new-user',
      flags: ['approved', 'displayname', 'email', 'firstname', 'lastname', 'notify', 'password', 'username']
    },
    {name: 'purge-user', flags: ['id']},
    {name: 'reset-password', flags: ['id', 'notify']},
    {name: 'restore-user', flags: ['id']}
  ];
};