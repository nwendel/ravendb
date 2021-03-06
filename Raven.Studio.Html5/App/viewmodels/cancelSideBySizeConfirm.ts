import index = require("models/index");
import deleteIndexCommand = require("commands/deleteIndexCommand");
import dialog = require("plugins/dialog");
import database = require("models/database");
import dialogViewModelBase = require("viewmodels/dialogViewModelBase");
import messagePublisher = require("common/messagePublisher");

class cancelSideBySizeConfirm extends dialogViewModelBase {

    cancelTask = $.Deferred();
    title: string;

    constructor(private indexNames: string[], private db: database, title?) {
        super();

        if (!indexNames || indexNames.length === 0) {
            throw new Error("Indexes must not be null or empty.");
        }

        this.title = !!title ? title : indexNames.length == 1 ? 'Cancel side-by-side index?' : 'Cancel side-by-side indexes?';
    }

    cancelIndexes() {
        var cancelTasks = this.indexNames.map(name => new deleteIndexCommand(name, this.db).execute());
        var myDeleteTask = this.cancelTask;

        $.when.apply($, cancelTasks)
            .done(() => {
                if (this.indexNames.length > 1) {
                    messagePublisher.reportSuccess("Successfully cancelled " + this.indexNames.length + " index replacements!");
                }
                myDeleteTask.resolve(false);
            })
            .fail(()=> {
                myDeleteTask.reject();
            });
        dialog.close(this);
    }

    cancel() {
        this.cancelTask.reject();
        dialog.close(this);
    }
}

export = cancelSideBySizeConfirm;