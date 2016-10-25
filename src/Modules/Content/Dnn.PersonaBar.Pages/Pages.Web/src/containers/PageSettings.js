import React, {Component, PropTypes } from "react";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import Tabs from "dnn-tabs";
import Localization from "../localization";
import GridSystem from "dnn-grid-system";
import EditableField from "dnn-editable-field";
import SocialPanelBody from "dnn-social-panel-body";
import SingleLineInputWithError from "dnn-single-line-input-with-error";

class PageSettings extends Component {

    render() {
        const {selectedPage} = this.props;

        return (
            <SocialPanelBody>
                <Tabs tabHeaders={[Localization.get("Details"), Localization.get("Permissions")]}>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("Name")}
                            tooltipMessage={Localization.get("page_name_tooltip")}
                            value={selectedPage.name}/>
                        <SingleLineInputWithError
                            label={Localization.get("Title")}
                            tooltipMessage={Localization.get("page_title_tooltip")}
                            value={selectedPage.title}/>
                    </div>
                    <div>
                        <EditableField
                            label={Localization.get("Description")}
                            value={selectedPage.description}
                            editable={true} />
                    </div>
                    <div>
                        <EditableField
                            label={Localization.get("Keywords")}
                            value={selectedPage.keywords}
                            editable={true} />
                    </div>
                    <div>
                        <EditableField
                            label={Localization.get("Tags")}
                            value={selectedPage.tags}
                            editable={true} />
                        <EditableField
                            label={Localization.get("URL")}
                            value={selectedPage.url}
                            editable={false} />
                    </div>
                </Tabs>
            </SocialPanelBody>
        );
    }    
    
}

PageSettings.propTypes = {
    selectedPage: PropTypes.object
};

export default PageSettings;

