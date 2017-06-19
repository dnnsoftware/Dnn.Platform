import React, {Component, PropTypes } from "react";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import InputGroup from "dnn-input-group";
import SingleLineInput from "dnn-single-line-input";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import Localization from "../../../localization";
import styles from "./style.less";
import Tags from "dnn-tags";
import Label from "dnn-label";

class PageDetails extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    onChangeTags(tags) {
        const {onChangeField} = this.props;
        onChangeField("tags", tags.join(","));
    }

    onChangeUrl(event) {
        const {onChangeField} = this.props;
        let value = event.target.value;
        if (!value.startsWith("/")) {
            value = "/" + value;
        }  
        onChangeField("url", value);
    }

    render() {
        const {page, errors} = this.props;
        const tags = page.tags ? page.tags.split(",") : [];

        return (
            <div className={styles.pageStandard}>
                <GridSystem>
                    <GridCell className="left-column">
                        <SingleLineInputWithError
                            label={Localization.get("Name") + "*"}
                            tooltipMessage={Localization.get("NameTooltip")}
                            error={!!errors.name}
                            errorMessage={errors.name}
                            value={page.name} 
                            onChange={this.onChangeField.bind(this, "name")}
                            maxLength="200" />
                    </GridCell>
                    <GridCell className="right-column">
                        <SingleLineInputWithError
                            label={Localization.get("Title")}
                            tooltipMessage={Localization.get("TitleTooltip")}
                            value={page.title}
                            onChange={this.onChangeField.bind(this, "title")}
                            maxLength="200" />
                    </GridCell>
                </GridSystem>
                <InputGroup>
                    <MultiLineInputWithError
                        label={Localization.get("Description")}
                        value={page.description}
                        onChange={this.onChangeField.bind(this, "description")}
                        maxLength="500" />
                </InputGroup>
                <InputGroup>
                    <MultiLineInputWithError
                        label={Localization.get("Keywords")}
                        value={page.keywords} 
                        onChange={this.onChangeField.bind(this, "keywords")}
                        maxLength="500" />
                </InputGroup>
                <GridSystem>
                    <GridCell className="left-column input-cell">
                        <Label label={Localization.get("Tags")}/>
                        <Tags 
                            tags={tags} 
                            addTagsPlaceholder={Localization.get("addTagsPlaceholder")}
                            onUpdateTags={this.onChangeTags.bind(this)}/>
                    </GridCell>
                    <GridCell className="right-column input-cell">
                        <Label label={Localization.get("Url")}/>
                        <SingleLineInput 
                            value={page.url}
                            onChange={this.onChangeUrl.bind(this)} />
                    </GridCell>
                </GridSystem>
                <div style={{clear: "both"}}></div>
            </div>
        );
    }

}

PageDetails.propTypes = {
    page: PropTypes.object.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageDetails;