import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Tabs from "dnn-tabs";
import {
    pagination as PaginationActions
} from "../../actions";
import SocialPanelBody from "dnn-social-panel-body";
import "./style.less";

class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);        
    }   

    handleSelect(index/*, last*/) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    render() {
        const {props} = this;

        return (
            <SocialPanelBody>
                <Tabs onSelect={this.handleSelect}
                    selectedIndex={props.tabIndex}
                    tabHeaders={["System Info", "Server Settings"]}>
                    <Tabs
                        tabHeaders={["Application", "Web", "Database"]}
                        type="secondary">
                        <div>
                            <h1>Application</h1>
                        </div>
                        <div>
                            <h1>Web</h1>
                        </div>
                        <div>
                            <h1>Database</h1>
                        </div>
                    </Tabs>
                    <Tabs
                        tabHeaders={["Smtp Server", "Performance", "Logs"]}
                        type="secondary">          
                        <div>
                            <h1>Smtp Server</h1>
                        </div>
                        <div>
                            <h1>Performance</h1>
                        </div>
                        <div>
                            <h1>Logs</h1>
                        </div>              
                    </Tabs>                    
                </Tabs>                
            </SocialPanelBody >
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(Body);