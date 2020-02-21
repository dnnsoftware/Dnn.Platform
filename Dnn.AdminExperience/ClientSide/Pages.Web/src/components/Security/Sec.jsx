import { Component } from "react";
import PropTypes from "prop-types";
import securityService from "../../services/securityService";

class Sec extends Component {

    componentDidMount() {

    }

    isVisible() {
        const { selectedPage, permission, onlySuperUsers, onlyForNotSuperUser } = this.props;

        const isSuperUser = securityService.isSuperUser();
        if (onlyForNotSuperUser && isSuperUser) {
            return false;
        }

        if (isSuperUser) {
            return true;
        }

        if (onlySuperUsers) {
            return false;
        }

        return securityService.userHasPermission(permission, selectedPage);
    }

    render() {
        const isVisible = this.isVisible();
        if (!isVisible) {
            return null;
        }

        return this.props.children;
    }
}

Sec.propTypes = {
    className: PropTypes.string,
    children: PropTypes.node,
    onlySuperUsers: PropTypes.bool,
    onlyForNotSuperUser: PropTypes.bool,
    permission: PropTypes.string,
    selectedPage: PropTypes.object
};

export default Sec;