import React, {Component, PropTypes} from "react";
import securityService from "../../services/securityService";

class Sec extends Component {

    componentDidMount() {
        
    }
    
    isVisible() {
        const { permission, onlySuperUsers } = this.props;
        
        const isSuperUser = securityService.isSuperUser();
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
        
        return (
            <div className={this.props.className}>
                {this.props.children}
            </div>
        );
    }
}

Sec.propTypes = {
    className: PropTypes.string,
    children: PropTypes.node,
    onlySuperUsers: PropTypes.bool,
    permission: PropTypes.string
};

export default Sec;