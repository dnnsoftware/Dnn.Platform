import React, { Component } from "react";
import PropTypes from "prop-types";
import { Button, Collapsible, GridCell, MultiLineInputWithError, SvgIcons, SingleLineInputWithError } from "@dnnsoftware/dnn-react-common";
import utils from "../../utils";
import resx from "../../resources";
import "./style.less";

class PageHeaderTags extends Component {
    constructor(props) {
        super(props);
        this.state = {
            addingNew: false,
            editingIndex: -1,
            draft: { name: "", content: "" },
            triedToSubmit: false
        };
    }

    onOpenNewForm() {
        this.setState({
            addingNew: true,
            editingIndex: -1,
            draft: { name: "", content: "" },
            triedToSubmit: false
        });
    }

    onOpenEditForm(index) {
        const { value } = this.props;
        const opened = this.state.editingIndex === index;

        if (opened) {
            this.onCloseForm();
            return;
        }

        const item = (value || [])[index] || { name: "", content: "" };
        this.setState({
            addingNew: false,
            editingIndex: index,
            draft: { name: item.name || "", content: item.content || "" },
            triedToSubmit: false
        });
    }

    onCloseForm() {
        this.setState({
            addingNew: false,
            editingIndex: -1,
            draft: { name: "", content: "" },
            triedToSubmit: false
        });
    }

    onDelete(index) {
        const { onChange, value } = this.props;
        utils.utilities.confirm(resx.get("PageHeaderTags_DeleteConfirm"), resx.get("PageHeaderTags_Delete"), resx.get("Cancel"), () => {
            onChange((value || []).filter((item, itemIndex) => itemIndex !== index));
            if (this.state.editingIndex === index) {
                this.onCloseForm();
            }
        });
    }

    onChangeField(key, event) {
        this.setState({
            draft: {
                ...this.state.draft,
                [key]: event.target.value
            }
        });
    }

    hasDuplicateName() {
        const { value } = this.props;
        const { draft, editingIndex } = this.state;
        const normalizedName = (draft.name || "").trim().toLowerCase();

        if (!normalizedName) {
            return false;
        }

        return (value || []).some((item, index) => index !== editingIndex && ((item.name || "").trim().toLowerCase() === normalizedName));
    }

    hasNameTooLong() {
        return ((this.state.draft.name || "").trim().length > 30);
    }

    onSave() {
        const { onChange, value } = this.props;
        const { addingNew, editingIndex, draft } = this.state;
        const nextDraft = {
            name: (draft.name || "").trim(),
            content: draft.content || ""
        };

        this.setState({ triedToSubmit: true, draft: nextDraft });

        if (!nextDraft.name || !nextDraft.content.trim() || this.hasDuplicateName() || this.hasNameTooLong()) {
            return;
        }

        const nextValue = [...(value || [])];
        if (addingNew) {
            nextValue.push(nextDraft);
        }
        else if (editingIndex >= 0) {
            nextValue[editingIndex] = nextDraft;
        }

        onChange(nextValue);
        this.onCloseForm();
    }

    renderEditor(isOpened) {
        const { draft, triedToSubmit } = this.state;
        const duplicateName = this.hasDuplicateName();
        const nameTooLong = this.hasNameTooLong();

        return <Collapsible accordion={true} isOpened={isOpened} className="editTag">
            <div className="editTag-body">
                <GridCell>
                    <GridCell columnSize={60}>
                        <SingleLineInputWithError
                            style={{ width: "100%" }}
                            label={resx.get("PageHeaderTags_Name")}
                            value={draft.name}
                            onChange={this.onChangeField.bind(this, "name")}
                            maxLength={30}
                            error={triedToSubmit && (!draft.name || duplicateName || nameTooLong)}
                            errorMessage={!draft.name ? resx.get("PageHeaderTags_NameRequired") : duplicateName ? resx.get("PageHeaderTags_NameDuplicate") : resx.get("PageHeaderTags_NameTooLong")}
                        />
                    </GridCell>
                    <GridCell columnSize={95}>
                        <MultiLineInputWithError
                            label={resx.get("PageHeaderTags_Content")}
                            value={draft.content}
                            onChange={this.onChangeField.bind(this, "content")}
                            error={triedToSubmit && !draft.content.trim()}
                            errorMessage={resx.get("PageHeaderTags_ContentRequired")}
                        />
                    </GridCell>
                </GridCell>
                <div className="buttons-box" style={{ float: "left", margin: "0 0 20px 0" }}>
                    <Button type="secondary" onClick={this.onCloseForm.bind(this)}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button type="primary" onClick={this.onSave.bind(this)}>
                        {resx.get("Save")}
                    </Button>
                </div>
                <div style={{ clear: "both" }} />
            </div>
        </Collapsible>;
    }

    renderAddRow() {
        const { addingNew } = this.state;

        if (!addingNew) {
            return null;
        }

        return <div className="tagRow row-opened">
            <GridCell columnSize={90}>-</GridCell>
            <GridCell columnSize={10}>
                <div className="extension-action-hidden" />
            </GridCell>
            {this.renderEditor(true)}
        </div>;
    }

    renderRows() {
        const { value } = this.props;
        const { editingIndex } = this.state;

        if (!value || value.length === 0) {
            return null;
        }

        return value.map((item, index) => {
            const opened = editingIndex === index;

            return <div className={`tagRow${opened ? " row-opened" : ""}`} key={`${item.name || "tag"}-${index}`}>
                <GridCell columnSize={90} className="tag-name">
                    {item.name}
                </GridCell>
                <GridCell columnSize={10}>
                    {!opened && <div className="extension-action" onClick={this.onDelete.bind(this, index)}><SvgIcons.TrashIcon /></div>}
                    {!opened && <div className="extension-action" onClick={this.onOpenEditForm.bind(this, index)}><SvgIcons.EditIcon /></div>}
                </GridCell>
                {opened && this.renderEditor(true)}
            </div>;
        });
    }

    render() {
        const { label } = this.props;
        const { addingNew } = this.state;

        return <div className="page-header-tags">
            <div className="addItemRow">
                <div className="sectionTitle">{label}</div>
                <div className={`AddItemBox${addingNew ? " active" : ""}`} onClick={!addingNew ? this.onOpenNewForm.bind(this) : undefined}>
                    <div className={`add-icon${addingNew ? " active" : ""}`}><SvgIcons.AddIcon /></div>
                    {resx.get("PageHeaderTags_AddTag")}
                </div>
            </div>
            <div className="tag-table">
                <div className="header-row">
                    <GridCell columnSize={90}>{resx.get("PageHeaderTags_Name")}</GridCell>
                    <GridCell columnSize={10} />
                </div>
                {this.renderAddRow()}
                {this.renderRows()}
            </div>
        </div>;
    }
}

PageHeaderTags.propTypes = {
    label: PropTypes.string.isRequired,
    onChange: PropTypes.func.isRequired,
    value: PropTypes.array
};

export default PageHeaderTags;
