import React, {Component, PropTypes } from "react";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import InputGroup from "dnn-input-group";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import localization from "../../../localization";
import styles from "./style.less";

class PageDetails extends Component {

    onChangeField(key, event) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    render() {
        const {page, errors} = this.props;

        return (
            <div className={styles.pageStandard}>
                <GridSystem>
                    <GridCell className="left-column">
                        <SingleLineInputWithError
                            label={localization.get("Name") + "*"}
                            tooltipMessage={localization.get("page_name_tooltip")}
                            error={!!errors.name}
                            errorMessage={errors.name}
                            value={page.name} 
                            onChange={this.onChangeField.bind(this, "name")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <SingleLineInputWithError
                            label={localization.get("Title")}
                            tooltipMessage={localization.get("page_title_tooltip")}
                            value={page.title}
                            onChange={this.onChangeField.bind(this, "title")} />
                    </GridCell>
                </GridSystem>
                <InputGroup>
                    <MultiLineInputWithError
                        label={localization.get("Description")}
                        value={page.description}
                        onChange={this.onChangeField.bind(this, "description")} />
                </InputGroup>
                <InputGroup>
                    <MultiLineInputWithError
                        label={localization.get("Keywords")}
                        value={page.keywords} 
                        onChange={this.onChangeField.bind(this, "keywords")} />
                </InputGroup>
                <GridSystem>
                    <GridCell className="left-column">
                        <SingleLineInputWithError
                            label={localization.get("Tags")}
                            value={page.tags} 
                            onChange={this.onChangeField.bind(this, "tags")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <SingleLineInputWithError
                            label={localization.get("URL")}
                            value={page.url}
                            enabled={false} />
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