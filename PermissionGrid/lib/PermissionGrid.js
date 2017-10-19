"use strict";

Object.defineProperty(exports, "__esModule", {
    value: true
});

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

var _react = require("react");

var _react2 = _interopRequireDefault(_react);

var _dnnGridCell = require("dnn-grid-cell");

var _dnnGridCell2 = _interopRequireDefault(_dnnGridCell);

var _dnnButton = require("dnn-button");

var _dnnButton2 = _interopRequireDefault(_dnnButton);

var _dnnLabel = require("dnn-label");

var _dnnLabel2 = _interopRequireDefault(_dnnLabel);

var _Grid = require("./Grid");

var _Grid2 = _interopRequireDefault(_Grid);

require("./style.less");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _possibleConstructorReturn(self, call) { if (!self) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return call && (typeof call === "object" || typeof call === "function") ? call : self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function, not " + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass; }

var defaultLocalization = {
    filterByGroup: "Filter By Group:",
    permissionsByRole: "Permissions By Role",
    permissionsByUser: "Permissions By User",
    addRolePlaceHolder: "Begin typing to add a role",
    addUserPlaceHolder: "Begin typing to add a user",
    addRole: "Add",
    addUser: "Add",
    emptyRole: "Add a role to set permissions by role",
    emptyUser: "Add a user to set permissions by user",
    globalGroupsText: "[Global Roles]",
    allGroupsText: "[All Roles]",
    roleText: "Role",
    userText: "User"
};

var PermissionGrid = function (_Component) {
    _inherits(PermissionGrid, _Component);

    function PermissionGrid(props) {
        _classCallCheck(this, PermissionGrid);

        var _this = _possibleConstructorReturn(this, (PermissionGrid.__proto__ || Object.getPrototypeOf(PermissionGrid)).call(this, props));

        _this.state = {
            definitions: props.permissions.permissionDefinitions,
            rolePermissions: props.permissions.rolePermissions,
            userPermissions: props.permissions.userPermissions,
            localization: Object.assign({}, defaultLocalization, props.localization)
        };
        return _this;
    }

    _createClass(PermissionGrid, [{
        key: "componentWillMount",
        value: function componentWillMount() {
            var props = this.props,
                state = this.state;
        }
    }, {
        key: "componentWillReceiveProps",
        value: function componentWillReceiveProps(newProps) {
            this.setState({
                definitions: newProps.permissions.permissionDefinitions,
                rolePermissions: newProps.permissions.rolePermissions,
                userPermissions: newProps.permissions.userPermissions
            });
        }
    }, {
        key: "localize",
        value: function localize(key) {
            var props = this.props,
                state = this.state;


            return state.localization[key] || key;
        }
    }, {
        key: "onPermissionsChanged",
        value: function onPermissionsChanged(type, permissions) {
            var props = this.props,
                state = this.state;


            var newState = { rolePermissions: state.rolePermissions, userPermissions: state.userPermissions };
            switch (type) {
                case "role":
                    newState = Object.assign(newState, { rolePermissions: permissions });
                    break;
                case "user":
                    newState = Object.assign(newState, { userPermissions: permissions });
                    break;
            }

            this.setState(newState, function () {
                if (typeof props.onPermissionsChanged === "function") {
                    props.onPermissionsChanged(newState);
                }
            });
        }
    }, {
        key: "onAddPermission",
        value: function onAddPermission(type, permission) {
            var props = this.props,
                state = this.state;


            var newState = { rolePermissions: state.rolePermissions, userPermissions: state.userPermissions };
            switch (type) {
                case "role":
                    newState.rolePermissions.push(permission);
                    break;
                case "user":
                    newState.userPermissions.push(permission);
                    break;
            }

            this.setState(newState, function () {
                if (typeof props.onPermissionsChanged === "function") {
                    props.onPermissionsChanged(newState);
                }
            });
        }
    }, {
        key: "renderRolesGrid",
        value: function renderRolesGrid() {
            var props = this.props,
                state = this.state;


            return _react2.default.createElement(_Grid2.default, {
                service: props.service,
                localization: state.localization,
                type: "role",
                definitions: state.definitions,
                permissions: state.rolePermissions,
                onChange: this.onPermissionsChanged.bind(this, "role"),
                onAddPermission: this.onAddPermission.bind(this, "role") });
        }
    }, {
        key: "renderUsersGrid",
        value: function renderUsersGrid() {
            var props = this.props,
                state = this.state;


            return _react2.default.createElement(_Grid2.default, {
                service: props.service,
                localization: state.localization,
                type: "user",
                definitions: state.definitions,
                permissions: state.userPermissions,
                onChange: this.onPermissionsChanged.bind(this, "user"),
                onAddPermission: this.onAddPermission.bind(this, "user") });
        }
    }, {
        key: "render",
        value: function render() {
            var props = this.props,
                state = this.state;


            if (!props.permissions || !props.permissions.permissionDefinitions) {
                return null;
            }

            return _react2.default.createElement(
                _dnnGridCell2.default,
                { className: "permissions-grid" + (props.className ? " " + props.className : "") },
                this.renderRolesGrid(),
                this.renderUsersGrid()
            );
        }
    }]);

    return PermissionGrid;
}(_react.Component);

PermissionGrid.propTypes = {
    dispatch: _react.PropTypes.func.isRequired,
    permissions: _react.PropTypes.object.isRequired,
    localization: _react.PropTypes.object,
    className: _react.PropTypes.string,
    service: _react.PropTypes.object.isRequired,
    onPermissionsChanged: _react.PropTypes.func.isRequired
};

PermissionGrid.DefaultProps = {};

exports.default = PermissionGrid;
//# sourceMappingURL=PermissionGrid.js.map