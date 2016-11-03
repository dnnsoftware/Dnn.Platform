import React, {Component, PropTypes} from "react";
import PageStandard from "./PageStandard/PageStandard";
import PageExisting from "./PageExisting/PageExisting";
import PageDetailsFooter from "./PageDetailsFooter/PageDetailsFooter";

class PageDetail extends Component {

    getDetail(pageType) {        
        switch (pageType) {
            case "normal": return PageStandard;
            case "tab": return PageExisting;
            default: throw "invalid page type";                                                                        
        }        
    }

    render() {
        const DetailComponent = this.getDetail(this.props.page.pageType);
        return (
            <div>
                <DetailComponent {...this.props} />
                <PageDetailsFooter {...this.props} />
            </div>
        );
    }
}

PageDetail.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageDetail;