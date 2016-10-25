import React, {Component, PropTypes } from "react";
import Tabs from "dnn-tabs";
import Localization from "../localization";
import SocialPanelBody from "dnn-social-panel-body";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";

class PageSettings extends Component {

    render() {
        const {selectedPage} = this.props;

        return (
            <SocialPanelBody>
                <Tabs tabHeaders={[Localization.get("Details"), Localization.get("Permissions")]}>
                    <div>
                        <GridSystem>
                            
                                <SingleLineInputWithError
                                    label={Localization.get("Name")}
                                    tooltipMessage={Localization.get("page_name_tooltip")}
                                    value={selectedPage.name}/>
                                <SingleLineInputWithError
                                    label={Localization.get("Title")}
                                    tooltipMessage={Localization.get("page_title_tooltip")}
                                    value={selectedPage.title}/>
                            
                        </GridSystem>
                        <GridCell>
                            <MultiLineInputWithError
                                label={Localization.get("Description")}
                                value={selectedPage.description} />
                        </GridCell>
                        <GridCell>
                            <MultiLineInputWithError
                                label={Localization.get("Keywords")}
                                value={selectedPage.keywords} />
                        </GridCell>
                        <GridCell>
                            <SingleLineInputWithError
                                label={Localization.get("Tags")}
                                value={selectedPage.tags} />
                            <SingleLineInputWithError
                                label={Localization.get("URL")}
                                value={selectedPage.url}
                                enabled={false} />
                        </GridCell>
                    </div>
                    <div>
                        <p>Permission stuff</p>
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

