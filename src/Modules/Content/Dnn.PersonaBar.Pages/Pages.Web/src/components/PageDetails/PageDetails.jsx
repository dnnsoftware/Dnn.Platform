import React, {Component, PropTypes} from "react";
import PageStandard from "./PageStandard/PageStandard";

class PageDetail extends Component {

    getDetail(pageType) {        
        switch (pageType) {
            case "normal": return PageStandard;
            default: throw "invalid page type";                                                                        
        }        
    }

    render() {
        const DetailComponent = this.getDetail(this.props.page.pageType);
        return <DetailComponent {...this.props} />;
    }
}

PageDetail.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageDetail;