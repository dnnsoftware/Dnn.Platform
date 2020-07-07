import React, {Component} from "react";
import PropTypes from "prop-types";
import {connect} from "react-redux";
import PageStandard from "./PageStandard/PageStandard";
import PageUrl from "./PageUrl/PageUrl";
import PageDetailsFooter from "./PageDetailsFooter/PageDetailsFooter";
import PageIcons from "./PageIcons/PageIcons";

class PageDetail extends Component {

    getDetail(pageType) {        
        switch (pageType) {
            case "normal": 
                return PageStandard;
            case "tab":
            case "url":
            case "file":
                return PageUrl;
            default: 
                throw "invalid page type";
        }        
    }

    render() {
        const DetailComponent = this.getDetail(this.props.page.pageType);
        return (
            <div>
                <DetailComponent onChangeField={this.props.onChangeField} errors={this.props.errors} onSelectParentPageId={this.props.onSelectParentPageId}/>
                <PageIcons components={this.props.components} onChangeField={this.props.onChangeField} errors={this.props.errors} page={this.props.page} validationCode={this.props.page.validationCode} />
                <PageDetailsFooter components={this.props.components} onChangeField={this.props.onChangeField} errors={this.props.errors} />
            </div>
        );
    }
}

PageDetail.propTypes = {
    page: PropTypes.object.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    components: PropTypes.array.isRequired,
    onSelectParentPageId: PropTypes.func.isRequired
};

const mapStateToProps = (state) => {
    return {
        page: state.pages.selectedPage        
    };
};

export default connect(mapStateToProps)(PageDetail);