import {Component, PropTypes} from "react";
import securityService from "../../services/securityService";

class Sec extends Component {

    componentDidMount() {
        
    }
    
    isVisible() {
        const { permission, onlySuperUsers, onlyForNotSuperUser } = this.props;
                
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
        
        return securityService.userHasPermission(permission);
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
    permission: PropTypes.string
};

export default Sec;