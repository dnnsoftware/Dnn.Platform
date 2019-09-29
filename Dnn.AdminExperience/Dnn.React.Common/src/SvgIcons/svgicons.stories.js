import React from "react";
import { storiesOf } from "@storybook/react";
import * as SvgIcons from "./index";
import GridSystem from "../GridSystem";

storiesOf("SvgIcons", module).add("with content", () => (
    <div>
        <div>ActivityIcon <div style={{width: "24px" }} dangerouslySetInnerHTML={{ __html: SvgIcons.ActivityIcon }}></div></div>
        <div>AddIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.AddIcon }}></div></div>
        <div>AddCircleIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.AddCircleIcon }}></div></div>
        <div>EditIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.EditIcon }}></div></div>
        <div>CardViewIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CardViewIcon }}></div></div>
        <div>ListViewIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ListViewIcon }}></div></div>
        <div>PreviewIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.PreviewIcon }}></div></div>
        <div>SettingsIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.SettingsIcon }}></div></div>
        <div>PageIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.PageIcon }}></div></div>
        <div>TrafficIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TrafficIcon }}></div></div>
        <div>TemplateIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TemplateIcon }}></div></div>
        <div>TrashIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TrashIcon }}></div></div>
        <div>UserIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.UserIcon }}></div></div>
        <div>UsersIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.UsersIcon }}></div></div>
        <div>ArrowDownIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowDownIcon }}></div></div>
        <div>ArrowRightIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowRightIcon }}></div></div>
        <div>ArrowUpIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowUpIcon }}></div></div>
        <div>ArrowLeftIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowLeftIcon }}></div></div>
        <div>DoubleArrowRightIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.DoubleArrowRightIcon }}></div></div>
        <div>DoubleArrowLeftIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.DoubleArrowLeftIcon }}></div></div>
        <div>ArrowEndLeftIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowEndLeftIcon }}></div></div>
        <div>ArrowEndRightIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowEndRightIcon }}></div></div>
        <div>CheckboxCheckedIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CheckboxCheckedIcon }}></div></div>
        <div>CheckboxPartialCheckedIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CheckboxPartialCheckedIcon }}></div></div>
        <div>CheckboxUncheckedIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CheckboxUncheckedIcon }}></div></div>
        <div>CalendarIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CalendarIcon }}></div></div>
        <div>CalendarEndIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CalendarEndIcon }}></div></div>
        <div>CalendarStartIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CalendarStartIcon }}></div></div>
        <div>CheckboxIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CheckboxIcon }}></div></div>
        <div>CheckMarkIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CheckMarkIcon }}></div></div>
        <div>ClockStopIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ClockStopIcon }}></div></div>
        <div>CrossOutIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CrossOutIcon }}></div></div>
        <div>CycleIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CycleIcon }}></div></div>
        <div>DragRowIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.DragRowIcon }}></div></div>
        <div>ErrorStateIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ErrorStateIcon }}></div></div>
        <div>EyeIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.EyeIcon }}></div></div>
        <div>FolderIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.FolderIcon }}></div></div>
        <div>GlobalIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.GlobalIcon }}></div></div>
        <div>HistoryIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.HistoryIcon }}></div></div>
        <div>LanguagesIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.LanguagesIcon }}></div></div>
        <div>LanguagesPageIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.LanguagesPageIcon }}></div></div>
        <div>LinkIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.LinkIcon }}></div></div>
        <div>LockClosedIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.LockClosedIcon }}></div></div>
        <div>MoreMenuIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.MoreMenuIcon }}></div></div>
        <div>SiteGroupNoData <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.SiteGroupNoData }}></div></div>
        <div>PagesIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.PagesIcon }}></div></div>
        <div>SearchFileSystemIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.SearchFileSystemIcon }}></div></div>
        <div>SearchIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.SearchIcon }}></div></div>
        <div>ShieldIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ShieldIcon }}></div></div>
        <div>Signature <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.Signature }}></div></div>
        <div>SiteGroupIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.SiteGroupIcon }}></div></div>
        <div>Steps1Icon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.Steps1Icon }}></div></div>
        <div>Steps2Icon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.Steps2Icon }}></div></div>
        <div>Steps3Icon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.Steps3Icon }}></div></div>
        <div>ToolTipIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ToolTipIcon }}></div></div>
        <div>UploadIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.UploadIcon }}></div></div>
        <div>UploadCircleIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.UploadCircleIcon }}></div></div>
        <div>XIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.XIcon }}></div></div>
        <div>XThinIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.XThinIcon }}></div></div>
        <div>ModuleIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ModuleIcon }}></div></div>
        <div>ArrowMoveUpIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowMoveUpIcon }}></div></div>
        <div>ArrowMoveDownIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowMoveDownIcon }}></div></div>
        <div>TableEmptyState <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TableEmptyState }}></div></div>
        <div>ArrowBack <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowBack }}></div></div>
        <div>ArrowForward <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowForward }}></div></div>
        <div>DownloadIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.DownloadIcon }}></div></div>
        <div>ImageIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ImageIcon }}></div></div>
        <div>HourglassIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.HourglassIcon }}></div></div>
        <div>CollapseTree <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.CollapseTree }}></div></div>
        <div>ExpandTree <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.ExpandTree }}></div></div>
        <div>TreeLinkIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TreeLinkIcon }}></div></div>
        <div>TreeDraftIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TreeDraftIcon }}></div></div>
        <div>TreeMoreIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TreeMoreIcon }}></div></div>
        <div>TreePaperClip <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TreePaperClip }}></div></div>
        <div>TreeAddPage <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TreeAddPage }}></div></div>
        <div>TreeEye <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TreeEye }}></div></div>
        <div>TreeEdit <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TreeEdit }}></div></div>
        <div>TreeCopy <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TreeCopy }}></div></div>
        <div>TreeAnalytics <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.TreeAnalytics }}></div></div>
        <div>UserSlash <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.UserSlash }}></div></div>
        <div>PagesSearchIcon <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.PagesSearchIcon }}></div></div>
        <div>PagesVerticalMore <div style={{width: "24px"}} dangerouslySetInnerHTML={{ __html: SvgIcons.PagesVerticalMore }}></div></div>
    </div>
));
