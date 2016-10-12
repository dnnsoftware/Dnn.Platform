import React, { Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import { connect } from "react-redux";
import SocialPanelBody from "dnn-social-panel-body";
import {
    pagination as PaginationActions,
    licensing as LicensingActions
} from "../../actions";
import Platform from "../platform";
import "./style.less";
import resx from "../../resources";

export class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    componentWillMount() {
        const {props} = this;
        props.dispatch(LicensingActions.getProduct());
    }    

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <SocialPanelBody>
            {this.props.productName === "DNNCORP.CE" &&
                <Platform/>
            }
            </SocialPanelBody>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    productName: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.index,
        productName: state.licensing.productName
    };
}

export default connect(mapStateToProps)(Body);